using MWMS.Domain.Common;

namespace MWMS.Domain.Entities;

public class AuditLog : BaseEntity
{
    public int? AdminUserId { get; set; }
    
    public int? TargetEmployeeId { get; set; } // Changed to nullable

    public string ActionType { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string OldValues { get; set; } = string.Empty;
    public string NewValues { get; set; } = string.Empty;
    
    public string Changes { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
