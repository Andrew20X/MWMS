namespace MWMS.Domain.Enums;

/// <summary>
/// Standardized leave types with their official codes.
/// </summary>
public enum LeaveType
{
    Annual = 1,    // RDO – Annual Leave (15 days per year)
    Emergency = 2, // EDO – Emergency Leave (6 days per year)
    Sick = 3,      // RSD – Reported Sick Day (no fixed annual limit)
    Absence = 4    // AWD – Absence Without Permission (recorded as absence)
}
