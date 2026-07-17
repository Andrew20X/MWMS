namespace MWMS.Application.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public int? EmployeeId { get; set; }
    
    public string FullName { get; set; } = string.Empty;
    
    public bool RequiresPasswordChange { get; set; }
}