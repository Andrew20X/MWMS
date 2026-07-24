using MWMS.Domain.Common;
using MWMS.Domain.Enums;
using System;

namespace MWMS.Domain.Entities;

public class SalaryDeduction : BaseEntity
{
    public int EmployeeId { get; set; }
    public int RelatedAttendanceId { get; set; }
    
    public decimal DeductionAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    
    public DateTime AppliedOnDate { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.PendingPayroll;
    
    public DateTime? RejectionDate { get; set; }
    public string? RejectionReason { get; set; }
    
    public Employee Employee { get; set; } = null!;
    public Attendance RelatedAttendance { get; set; } = null!;
}
