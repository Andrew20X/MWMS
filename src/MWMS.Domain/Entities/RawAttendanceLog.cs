using MWMS.Domain.Common;

namespace MWMS.Domain.Entities;

public class RawAttendanceLog : BaseEntity
{
    public int EmployeeId { get; set; }
    
    public DateTime PunchTime { get; set; }
    
    public string DeviceId { get; set; } = string.Empty;
    
    public bool IsProcessed { get; set; } = false;
    
    // Navigation Property
    public Employee Employee { get; set; } = null!;
}
