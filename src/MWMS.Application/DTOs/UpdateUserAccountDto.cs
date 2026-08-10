namespace MWMS.Application.DTOs;

public class UpdateUserAccountDto
{
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string? PositionName { get; set; }
    public string Role { get; set; } = "Employee";
    public int? ManagerId { get; set; }
    public List<int> SubordinateIds { get; set; } = new List<int>();
}
