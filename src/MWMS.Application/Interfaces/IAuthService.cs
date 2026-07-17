using MWMS.Application.DTOs;

namespace MWMS.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task CreateUserAsync(CreateUserRequest request);
    Task ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    Task ForceResetPasswordAsync(int employeeId);
    Task GenerateLoginsAsync();
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
}
