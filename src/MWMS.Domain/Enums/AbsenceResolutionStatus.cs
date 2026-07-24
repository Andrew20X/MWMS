namespace MWMS.Domain.Enums;

public enum AbsenceResolutionStatus
{
    None = 0,
    PendingResolution = 1,
    ResolvedWithLeave = 2,
    DeductionApplied = 3,
    Waived = 4,
    ExceptionRejected = 5
}
