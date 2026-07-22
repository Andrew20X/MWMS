namespace MWMS.Application.DTOs;

public class LiveAttendanceDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public string CheckInTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
