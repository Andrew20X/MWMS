namespace MWMS.Application.DTOs.Attendance;

public class CheckInResponseDto
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public TimeOnly CheckInTime { get; set; }

    public bool IsLate { get; set; }

    public int LateMinutes { get; set; }
}