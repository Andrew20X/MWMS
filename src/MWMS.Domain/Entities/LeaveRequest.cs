using MWMS.Domain.Common;
using MWMS.Domain.Enums;

namespace MWMS.Domain.Entities;

public class LeaveRequest : BaseEntity
{
    public int EmployeeId { get; set; }
    
    public LeaveType Type { get; set; }
    
    public DateOnly StartDate { get; set; }
    
    public DateOnly EndDate { get; set; }
    
    public string Reason { get; set; } = string.Empty;
    
    public LeaveStatus Status { get; set; } = LeaveStatus.PendingManagerApproval;
    
    public int? ApprovedById { get; set; }
    
    public string? AdminMessage { get; set; }

    // Approval tracking
    public int? ApprovedByManagerId { get; set; }
    public int? ApprovedByHRId { get; set; }
    public DateTime? ManagerApprovalDate { get; set; }
    public DateTime? HRApprovalDate { get; set; }
    
    // Navigation Properties
    public Employee Employee { get; set; } = null!;
    
    public User? ApprovedBy { get; set; }
    
    public int? LinkedAttendanceId { get; set; }
    public Attendance? LinkedAttendance { get; set; }
}
