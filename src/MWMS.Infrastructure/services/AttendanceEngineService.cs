using MWMS.Application.DTOs.Attendance;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Domain.Enums;

namespace MWMS.Infrastructure.Services;

public class AttendanceEngineService : IAttendanceEngineService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IGenericRepository<RawAttendanceLog> _rawLogRepository;

    public AttendanceEngineService(
        IEmployeeRepository employeeRepository,
        IAttendanceRepository attendanceRepository,
        IGenericRepository<RawAttendanceLog> rawLogRepository)
    {
        _employeeRepository = employeeRepository;
        _attendanceRepository = attendanceRepository;
        _rawLogRepository = rawLogRepository;
    }

    public async Task ProcessRawLogsAsync(List<RawPunchDto> logs)
    {
        // 1. Save all raw logs first
        foreach (var log in logs)
        {
            await _rawLogRepository.AddAsync(new RawAttendanceLog
            {
                EmployeeId = log.EmployeeId,
                PunchTime = log.PunchTime,
                DeviceId = log.DeviceId,
                IsProcessed = true
            });
        }
        await _rawLogRepository.SaveChangesAsync();

        // 2. Group by Employee and Date
        var groupedLogs = logs
            .GroupBy(l => new { l.EmployeeId, Date = DateOnly.FromDateTime(l.PunchTime) })
            .ToList();

        foreach (var group in groupedLogs)
        {
            var employeeId = group.Key.EmployeeId;
            var date = group.Key.Date;
            var punches = group.OrderBy(p => p.PunchTime).ToList();

            if (!punches.Any()) continue;

            var firstPunch = TimeOnly.FromDateTime(punches.First().PunchTime);
            var lastPunch = TimeOnly.FromDateTime(punches.Last().PunchTime);

            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                continue;
            }

            var shift = employee.Shift;

            // Find or create attendance record
            var attendance = await _attendanceRepository.GetByEmployeeAndDateAsync(employeeId, date);
            bool isNew = false;
            if (attendance == null)
            {
                attendance = new Attendance
                {
                    EmployeeId = employeeId,
                    Date = date
                };
                isNew = true;
            }

            // 3. Process Check-In and Check-Out
            attendance.CheckIn = firstPunch;

            if (punches.Count > 1)
            {
                var timeDiff = lastPunch.ToTimeSpan() - firstPunch.ToTimeSpan();
                if (timeDiff.TotalMinutes >= 15)
                {
                    attendance.CheckOut = lastPunch;
                }
                else
                {
                    attendance.CheckOut = null;
                }
            }
            else
            {
                // Only one punch or punches are too close together. Might be missing a checkout.
                attendance.CheckOut = null;
            }

            // 4. Calculate statuses (Late, Overtime, Early Leave)
            if (shift != null && attendance.CheckIn > shift.StartTime.AddMinutes(shift.GraceMinutes))
            {
                attendance.Status = AttendanceStatus.Late;
                attendance.LateMinutes = (int)(attendance.CheckIn.Value.ToTimeSpan() - shift.StartTime.ToTimeSpan()).TotalMinutes;
            }
            else
            {
                attendance.Status = AttendanceStatus.Present;
                attendance.LateMinutes = 0;
            }

            if (attendance.CheckOut.HasValue)
            {
                attendance.WorkedHours = (attendance.CheckOut.Value.ToTimeSpan() - attendance.CheckIn.Value.ToTimeSpan()).TotalHours;

                if (shift != null)
                {
                    if (attendance.CheckOut.Value < shift.EndTime)
                    {
                        attendance.EarlyLeaveMinutes = (int)(shift.EndTime.ToTimeSpan() - attendance.CheckOut.Value.ToTimeSpan()).TotalMinutes;
                    }
                    else if (attendance.CheckOut.Value > shift.EndTime)
                    {
                        attendance.OvertimeMinutes = (int)(attendance.CheckOut.Value.ToTimeSpan() - shift.EndTime.ToTimeSpan()).TotalMinutes;
                    }
                }
            }

            if (isNew)
                await _attendanceRepository.AddAsync(attendance);
            else
                _attendanceRepository.Update(attendance);
        }

        await _attendanceRepository.SaveChangesAsync();
    }

    public async Task ProcessUnprocessedLogsAsync()
    {
        var allLogs = await _rawLogRepository.GetAllAsync();
        var unprocessedLogs = allLogs.Where(l => !l.IsProcessed).ToList();
        
        if (!unprocessedLogs.Any()) return;

        var groupedLogs = unprocessedLogs
            .GroupBy(l => new { l.EmployeeId, Date = DateOnly.FromDateTime(l.PunchTime) })
            .ToList();

        foreach (var group in groupedLogs)
        {
            var employeeId = group.Key.EmployeeId;
            var date = group.Key.Date;
            var punches = group.OrderBy(p => p.PunchTime).ToList();

            if (!punches.Any()) continue;

            var firstPunch = TimeOnly.FromDateTime(punches.First().PunchTime);
            var lastPunch = TimeOnly.FromDateTime(punches.Last().PunchTime);

            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                continue;
            }

            var shift = employee.Shift;

            var attendance = await _attendanceRepository.GetByEmployeeAndDateAsync(employeeId, date);
            bool isNew = false;
            if (attendance == null)
            {
                attendance = new Attendance
                {
                    EmployeeId = employeeId,
                    Date = date
                };
                isNew = true;
            }

            if (!attendance.CheckIn.HasValue || firstPunch < attendance.CheckIn.Value)
            {
                attendance.CheckIn = firstPunch;
            }

            if (punches.Count > 1 || (attendance.CheckIn.HasValue && lastPunch > attendance.CheckIn.Value))
            {
                var timeDiff = lastPunch.ToTimeSpan() - attendance.CheckIn!.Value.ToTimeSpan();
                if (timeDiff.TotalMinutes >= 15)
                {
                    if (!attendance.CheckOut.HasValue || lastPunch > attendance.CheckOut.Value)
                    {
                        attendance.CheckOut = lastPunch;
                    }
                }
            }

            if (shift != null && attendance.CheckIn.HasValue && attendance.CheckIn.Value > shift.StartTime.AddMinutes(shift.GraceMinutes))
            {
                attendance.Status = AttendanceStatus.Late;
                attendance.LateMinutes = (int)(attendance.CheckIn.Value.ToTimeSpan() - shift.StartTime.ToTimeSpan()).TotalMinutes;
            }
            else
            {
                attendance.Status = AttendanceStatus.Present;
                attendance.LateMinutes = 0;
            }

            if (attendance.CheckIn.HasValue && attendance.CheckOut.HasValue)
            {
                attendance.WorkedHours = (attendance.CheckOut.Value.ToTimeSpan() - attendance.CheckIn.Value.ToTimeSpan()).TotalHours;

                if (shift != null)
                {
                    if (attendance.CheckOut.Value < shift.EndTime)
                    {
                        attendance.EarlyLeaveMinutes = (int)(shift.EndTime.ToTimeSpan() - attendance.CheckOut.Value.ToTimeSpan()).TotalMinutes;
                    }
                    else if (attendance.CheckOut.Value > shift.EndTime)
                    {
                        attendance.OvertimeMinutes = (int)(attendance.CheckOut.Value.ToTimeSpan() - shift.EndTime.ToTimeSpan()).TotalMinutes;
                    }
                }
            }

            if (isNew)
                await _attendanceRepository.AddAsync(attendance);
            else
                _attendanceRepository.Update(attendance);
        }

        foreach (var log in unprocessedLogs)
        {
            log.IsProcessed = true;
            _rawLogRepository.Update(log);
        }

        await _attendanceRepository.SaveChangesAsync();
        await _rawLogRepository.SaveChangesAsync();
    }
}
