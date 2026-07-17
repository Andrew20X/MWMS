using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using MWMS.Application.DTOs.Attendance;
namespace MWMS.Persistence.Repositories;

public class AttendanceRepository : GenericRepository<Attendance>, IAttendanceRepository
{
    public AttendanceRepository(AppDbContext context)
        :base(context)
    {
    }

    public async Task<Attendance?> GetByEmployeeAndDateAsync(int employeeId, DateOnly date)
    {
          return await _context.Attendances
        .Include(a => a.Employee)
        .FirstOrDefaultAsync(a =>
            a.EmployeeId == employeeId &&
            a.Date == date);
    }

    public async Task<IEnumerable<Attendance>> GetByEmployeeAsync(int employeeId)
    {
        return await _context.Attendances
        .Include(a => a.Employee)
        .Where(a => a.EmployeeId == employeeId)
        .OrderBy(a => a.Date)
        .ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> GetTodayAttendanceAsync(DateOnly date)
    {
         return await _context.Attendances
        .Include(a => a.Employee)
        .Where(a => a.Date == date)
        .OrderBy(a => a.Employee.FirstName)
        .ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> GetAttendancesByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _context.Attendances
            .Include(a => a.Employee)
            .Where(a => a.Date >= startDate && a.Date <= endDate)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> GetRecentAttendanceAsync(int limit)
    {
        return await _context.Attendances
            .Include(a => a.Employee)
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.CheckIn)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> SearchAttendancesAsync(AttendanceFilterDto filter)
    {
        var query = _context.Attendances.Include(a => a.Employee).AsQueryable();

        if (filter.StartDate.HasValue)
            query = query.Where(a => a.Date >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(a => a.Date <= filter.EndDate.Value);

        if (!string.IsNullOrEmpty(filter.EmployeeName))
            query = query.Where(a => (a.Employee.FirstName + " " + a.Employee.LastName).Contains(filter.EmployeeName));

        if (!string.IsNullOrEmpty(filter.EmployeeCode))
            query = query.Where(a => a.Employee.EmployeeCode.Contains(filter.EmployeeCode));

        if (filter.DepartmentId.HasValue)
            query = query.Where(a => a.Employee.DepartmentId == filter.DepartmentId.Value);

        if (filter.Status.HasValue)
            query = query.Where(a => a.Status == filter.Status.Value);

        return await query.OrderByDescending(a => a.Date).ToListAsync();
    }
}