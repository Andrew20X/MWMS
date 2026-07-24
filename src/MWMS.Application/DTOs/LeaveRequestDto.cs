using MWMS.Domain.Enums;

namespace MWMS.Application.DTOs;

public class LeaveRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? AdminMessage { get; set; }

    /// <summary>Human-readable stage label for UI display (e.g. "Pending Manager Approval").</summary>
    public string StatusLabel { get; set; } = string.Empty;
    public int? LinkedAttendanceId { get; set; }
}
