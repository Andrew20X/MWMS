namespace MWMS.Application.DTOs.Attendance;

public class SubmittedTimesheetDto
{
    public string FileName { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public long FileSizeBytes { get; set; }
    public int CommentCount { get; set; }
    public string? LatestComment { get; set; }
}
