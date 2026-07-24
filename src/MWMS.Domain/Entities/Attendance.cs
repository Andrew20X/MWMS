using MWMS.Domain.Common;
using MWMS.Domain.Enums;

namespace MWMS.Domain.Entities;

public class Attendance : BaseEntity
{
    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    public DateOnly Date { get; set; }

    public TimeOnly? CheckIn { get; set; }

    public TimeOnly? CheckOut { get; set; }

    public AttendanceStatus Status { get; set; }

    public double WorkedHours { get; set; }

    public int LateMinutes { get; set; }

    public int EarlyLeaveMinutes { get; set; }

    public int OvertimeMinutes { get; set; }

    public string? Notes { get; set; }

    public bool IsUnexcused { get; set; }
    
    public AbsenceResolutionStatus AbsenceResolutionStatus { get; set; } = AbsenceResolutionStatus.None;
    
    public DateTime? DeadlineForLeaveRequest { get; set; }
}