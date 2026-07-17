namespace MWMS.Application.DTOs;

public class AttendanceTrendDto
{
    public string Date { get; set; } = string.Empty;
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
}
