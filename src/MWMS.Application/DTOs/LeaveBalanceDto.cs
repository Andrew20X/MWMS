namespace MWMS.Application.DTOs;

public class LeaveBalanceDto
{
    public int EmployeeId { get; set; }
    public int Year { get; set; }

    // Annual Leave (RDO)
    public int AnnualLeaveTotal { get; set; }
    public int AnnualLeaveUsed { get; set; }
    public int AnnualLeaveRemaining { get; set; }

    // Emergency Leave (EDO)
    public int EmergencyLeaveTotal { get; set; }
    public int EmergencyLeaveUsed { get; set; }
    public int EmergencyLeaveRemaining { get; set; }
}
