namespace MWMS.Application.DTOs.Attendance;

public class RawPunchDto
{
    public int EmployeeId { get; set; }
    public DateTime PunchTime { get; set; }
    public string DeviceId { get; set; } = string.Empty;
}
