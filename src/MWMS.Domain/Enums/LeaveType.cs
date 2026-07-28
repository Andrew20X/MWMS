namespace MWMS.Domain.Enums;

/// <summary>
/// Standardized leave types with their official codes.
/// </summary>
public enum LeaveType
{
    Annual = 1,    // RDO – Annual Leave (15 days per year)
    Emergency = 2, // EDO – Emergency Leave (6 days per year)
    Sick = 3,      // RSD – Reported Sick Day (no fixed annual limit)
    Absence = 4,   // AWD – Absence Without Permission (recorded as absence)
    OfficeDay = 5, // OD - Office Day
    OfficialHoliday = 6, // OH - Official Holiday
    ArrivalDay = 7, // Arv - Arrival Day
    FactoryDay = 8, // FD - Factory Day
    WeekEnd = 9,    // WE - Week End
    WorkWeekEnd = 10, // WWE - Work Week End
    EgyptFieldDay = 11, // EF - Egypt Field Day
    ClientOfficeDay = 12 // COD - Client Office Day
}
