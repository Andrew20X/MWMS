using MWMS.Domain.Entities;
using MWMS.Application.DTOs.Attendance;

namespace MWMS.Application.Interfaces;

public interface IAttendanceRepository : IGenericRepository<Attendance>
{
    Task<Attendance?> GetByEmployeeAndDateAsync(int employeeId, DateOnly date);

    Task<IEnumerable<Attendance>> GetByEmployeeAsync(int employeeId);

    Task<IEnumerable<Attendance>> GetTodayAttendanceAsync(DateOnly date);

    Task<IEnumerable<Attendance>> GetAttendancesByDateRangeAsync(DateOnly startDate, DateOnly endDate);
    
    Task<IEnumerable<Attendance>> GetRecentAttendanceAsync(int limit);
    
    Task<IEnumerable<Attendance>> SearchAttendancesAsync(AttendanceFilterDto filter);
}