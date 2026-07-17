using System;
using MWMS.Domain.Common;

namespace MWMS.Domain.Entities;

public class CorrectionRequest : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateOnly Date { get; set; }
    
    public TimeOnly? RequestedCheckIn { get; set; }
    public TimeOnly? RequestedCheckOut { get; set; }
    
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public string? AdminNote { get; set; }
}
