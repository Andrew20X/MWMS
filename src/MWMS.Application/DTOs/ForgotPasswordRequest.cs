namespace MWMS.Application.DTOs;

public class ForgotPasswordRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
