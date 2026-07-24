using MWMS.Domain.Common;

namespace MWMS.Domain.Entities;

public class AuditLog : BaseEntity
{
    public int? AdminUserId { get; set; }
    
    public int TargetEmployeeId { get; set; }
    
    public string Changes { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
