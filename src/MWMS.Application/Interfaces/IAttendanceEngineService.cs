using MWMS.Application.DTOs.Attendance;

namespace MWMS.Application.Interfaces;

public interface IAttendanceEngineService
{
    Task ProcessRawLogsAsync(List<RawPunchDto> logs);
    Task ProcessUnprocessedLogsAsync();
}
