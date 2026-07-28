using MWMS.Application.DTOs;

namespace MWMS.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetTodayStatsAsync();
    
    Task<IEnumerable<AttendanceTrendDto>> GetAttendanceTrendAsync(int days = 7);
    
    Task<IEnumerable<LiveAttendanceDto>> GetLiveAttendanceAsync();
    
    Task<IEnumerable<LiveAttendanceDto>> GetLateArrivalsTodayAsync();
    
    Task<IEnumerable<LiveAttendanceDto>> GetAbsentsTodayAsync();
}
