namespace MWMS.Domain.Enums;

/// <summary>
/// Tracks the two-stage approval workflow for leave requests.
/// </summary>
public enum LeaveStatus
{
    PendingManagerApproval = 1, // Initial state – awaiting Manager action
    Approved = 2,
    Rejected = 3,
    PendingHRApproval = 4       // Manager approved; awaiting HR final decision
}
