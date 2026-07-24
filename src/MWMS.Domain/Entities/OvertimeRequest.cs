using System;
using MWMS.Domain.Common;

namespace MWMS.Domain.Entities;

public class OvertimeRequest : BaseEntity
{
    // Status constants for the two-stage approval workflow
    public const string StatusPendingManager = "PendingManagerApproval";
    public const string StatusPendingHR = "PendingHRApproval";
    public const string StatusApproved = "Approved";
    public const string StatusRejected = "Rejected";

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public string Reason { get; set; } = string.Empty;
    
    public string Type { get; set; } = "WFH";

    /// <summary>Use OvertimeRequest.Status* constants. Default: PendingManagerApproval.</summary>
    public string Status { get; set; } = StatusPendingManager;

    public string? AdminNote { get; set; }

    // Approval tracking
    public int? ApprovedByManagerId { get; set; }
    public int? ApprovedByHRId { get; set; }
    public DateTime? ManagerApprovalDate { get; set; }
    public DateTime? HRApprovalDate { get; set; }
}
