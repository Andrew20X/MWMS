using MWMS.Domain.Common;

namespace MWMS.Domain.Entities;

/// <summary>
/// Tracks an employee's annual leave quota and usage per calendar year.
/// Auto-created with defaults when first accessed for a given year.
/// </summary>
public class LeaveBalance : BaseEntity
{
    public int EmployeeId { get; set; }

    public int Year { get; set; }

    // Annual Leave (RDO)
    public int AnnualLeaveTotal { get; set; } = 15;

    public int AnnualLeaveUsed { get; set; } = 0;

    // Emergency Leave (EDO)
    public int EmergencyLeaveTotal { get; set; } = 6;

    public int EmergencyLeaveUsed { get; set; } = 0;

    // Computed helpers (not stored)
    public int AnnualLeaveRemaining => AnnualLeaveTotal - AnnualLeaveUsed;

    public int EmergencyLeaveRemaining => EmergencyLeaveTotal - EmergencyLeaveUsed;

    // Navigation property
    public Employee Employee { get; set; } = null!;
}
