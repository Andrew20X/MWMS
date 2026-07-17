using MWMS.Domain.Enums;

namespace MWMS.Application.DTOs;

public class CreateLeaveRequestDto
{
    public int EmployeeId { get; set; }
    public LeaveType Type { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}
