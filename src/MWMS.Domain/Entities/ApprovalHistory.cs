using MWMS.Domain.Common;

namespace MWMS.Domain.Entities;

/// <summary>
/// Immutable audit record for each approval or rejection action taken on a Leave or Overtime request.
/// </summary>
public class ApprovalHistory : BaseEntity
{
    /// <summary>"Leave" or "Overtime"</summary>
    public string RequestType { get; set; } = string.Empty;

    /// <summary>Foreign key to the Leave or Overtime request ID.</summary>
    public int RequestId { get; set; }

    public int ApproverId { get; set; }

    public string ApproverName { get; set; } = string.Empty;

    /// <summary>"Manager" or "Admin" (HR)</summary>
    public string ApproverRole { get; set; } = string.Empty;

    /// <summary>"Approved" or "Rejected"</summary>
    public string Decision { get; set; } = string.Empty;

    public string? Comment { get; set; }

    public DateTime DecisionAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User Approver { get; set; } = null!;
}
