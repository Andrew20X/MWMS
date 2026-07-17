namespace MWMS.Application.DTOs;

public class UpdateLeaveBalanceDto
{
    public int Year { get; set; }
    public int AnnualLeaveTotal { get; set; }
    public int AnnualLeaveUsed { get; set; }
    public int EmergencyLeaveTotal { get; set; }
    public int EmergencyLeaveUsed { get; set; }
}
