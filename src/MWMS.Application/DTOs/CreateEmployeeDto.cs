namespace MWMS.Application.DTOs;

public class CreateEmployeeDto
{
    public string EmployeeCode { get; set; } = string.Empty;

    public int DeviceUserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string MiddleName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public int PositionId { get; set; }

    public int ShiftId { get; set; }
}