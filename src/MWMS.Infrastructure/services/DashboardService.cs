using MWMS.Application.DTOs;
using MWMS.Application.Interfaces;

namespace MWMS.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAttendanceRepository _attendanceRepository;

    public DashboardService(IEmployeeRepository employeeRepository, IAttendanceRepository attendanceRepository)
    {
        _employeeRepository = employeeRepository;
        _attendanceRepository = attendanceRepository;
    }

    public async Task<DashboardStatsDto> GetTodayStatsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Calculate Total Employees
        var allEmployees = await _employeeRepository.GetAllAsync();
        var totalEmployees = allEmployees.Count(e => e.IsActive);

        // Get today's attendance logs
        var todayAttendances = await _attendanceRepository.GetTodayAttendanceAsync(today);

        var presentCount = todayAttendances.Count();
        
        // Calculate late arrivals (CheckIn time > Shift StartTime + GraceMinutes)
        var lateCount = todayAttendances.Count(a => a.CheckIn.HasValue && a.Employee?.Shift?.StartTime != null && a.CheckIn.Value > a.Employee.Shift.StartTime.AddMinutes(a.Employee.Shift.GraceMinutes));

        // Calculate absentees
        var absentCount = totalEmployees - presentCount;
        if (absentCount < 0) absentCount = 0; // Guard

        return new DashboardStatsDto
        {
            TotalEmployees = totalEmployees,
            PresentToday = presentCount,
            LateArrivals = lateCount,
            Absent = absentCount
        };
    }

    public async Task<IEnumerable<AttendanceTrendDto>> GetAttendanceTrendAsync(int days = 7)
    {
        var trend = new List<AttendanceTrendDto>();
        var endDate = DateOnly.FromDateTime(DateTime.Today);
        var startDate = endDate.AddDays(-days + 1);

        var attendances = await _attendanceRepository.GetAttendancesByDateRangeAsync(startDate, endDate);
        var allEmployees = await _employeeRepository.GetAllAsync();
        var activeEmployeeCount = allEmployees.Count(e => e.IsActive && !e.IsDeleted);

        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var dailyAttendances = attendances.Where(a => a.Date == date && !a.IsDeleted).ToList();

            var presentCount = dailyAttendances.Count;
            // The property in Dashboard is a.CheckIn > a.Employee?.Shift?.StartTime + GraceMinutes. In Engine it sets a.Status = Late.
            var lateCount = dailyAttendances.Count(a => a.CheckIn.HasValue && a.Employee?.Shift?.StartTime != null && a.CheckIn.Value > a.Employee.Shift.StartTime.AddMinutes(a.Employee.Shift.GraceMinutes));
            var absentCount = activeEmployeeCount - presentCount;

            trend.Add(new AttendanceTrendDto
            {
                Date = date.ToString("MMM dd"),
                PresentCount = presentCount,
                AbsentCount = absentCount < 0 ? 0 : absentCount,
                LateCount = lateCount
            });
        }

        return trend;
    }

    public async Task<IEnumerable<LiveAttendanceDto>> GetLiveAttendanceAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayAttendances = await _attendanceRepository.GetTodayAttendanceAsync(today);

        // Filter employees who have checked in but not checked out (currently in office)
        var liveList = todayAttendances
            .Where(a => a.CheckIn.HasValue && !a.CheckOut.HasValue)
            .Select(a => new LiveAttendanceDto
            {
                EmployeeId = a.EmployeeId,
                EmployeeName = $"{a.Employee?.FirstName} {a.Employee?.LastName}",
                PositionName = a.Employee?.Position?.Name ?? "General",
                CheckInTime = a.CheckIn?.ToString("HH:mm") ?? "",
                Status = a.Status.ToString()
            })
            .OrderByDescending(a => a.CheckInTime)
            .ToList();

        return liveList;
    }
    public async Task<IEnumerable<LiveAttendanceDto>> GetLateArrivalsTodayAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayAttendances = await _attendanceRepository.GetTodayAttendanceAsync(today);

        var lateList = todayAttendances
            .Where(a => a.CheckIn.HasValue && a.Employee?.Shift?.StartTime != null && a.CheckIn.Value > a.Employee.Shift.StartTime.AddMinutes(a.Employee.Shift.GraceMinutes))
            .Select(a => new LiveAttendanceDto
            {
                EmployeeId = a.EmployeeId,
                EmployeeName = $"{a.Employee?.FirstName} {a.Employee?.LastName}",
                PositionName = a.Employee?.Position?.Name ?? "General",
                CheckInTime = a.CheckIn?.ToString("HH:mm") ?? "",
                Status = "Late"
            })
            .OrderByDescending(a => a.CheckInTime)
            .ToList();

        return lateList;
    }

    public async Task<IEnumerable<LiveAttendanceDto>> GetAbsentsTodayAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayAttendances = await _attendanceRepository.GetTodayAttendanceAsync(today);
        var allEmployees = await _employeeRepository.GetAllAsync();
        
        var presentEmployeeIds = todayAttendances.Select(a => a.EmployeeId).ToHashSet();
        
        var absents = allEmployees
            .Where(e => e.IsActive && !e.IsDeleted && !presentEmployeeIds.Contains(e.Id))
            .Select(e => new LiveAttendanceDto
            {
                EmployeeId = e.Id,
                EmployeeName = $"{e.FirstName} {e.LastName}",
                PositionName = e.Position?.Name ?? "General",
                CheckInTime = "",
                Status = "Absent"
            })
            .ToList();

        return absents;
    }
    public async Task<IEnumerable<LiveAttendanceDto>> GetPresentTodayAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayAttendances = await _attendanceRepository.GetTodayAttendanceAsync(today);

        var presentList = todayAttendances
            .Select(a => new LiveAttendanceDto
            {
                EmployeeId = a.EmployeeId,
                EmployeeName = $"{a.Employee?.FirstName} {a.Employee?.LastName}",
                PositionName = a.Employee?.Position?.Name ?? "General",
                CheckInTime = a.CheckIn?.ToString("HH:mm") ?? "",
                Status = a.Status.ToString()
            })
            .OrderByDescending(a => a.CheckInTime)
            .ToList();

        return presentList;
    }

    public async Task<IEnumerable<LiveAttendanceDto>> GetWorkforceAsync()
    {
        var allEmployees = await _employeeRepository.GetAllAsync();
        
        var workforce = allEmployees
            .Where(e => e.IsActive && !e.IsDeleted)
            .Select(e => new LiveAttendanceDto
            {
                EmployeeId = e.Id,
                EmployeeName = $"{e.FirstName} {e.LastName}",
                PositionName = e.Position?.Name ?? "General",
                CheckInTime = "",
                Status = "Active"
            })
            .OrderBy(e => e.EmployeeName)
            .ToList();

        return workforce;
    }
}
