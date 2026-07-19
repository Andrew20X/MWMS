using MWMS.Application.DTOs;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;

namespace MWMS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailService _emailService;

    public AuthService(
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailService = emailService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user == null)
            return null;

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;

        var employee = await _employeeRepository.GetByEmployeeCodeAsync(user.Username);
        if (employee == null && user.Username.StartsWith("EMP-SYNC-"))
        {
            var deviceUserId = user.Username.Substring("EMP-SYNC-".Length);
            employee = await _employeeRepository.GetByEmployeeCodeAsync(deviceUserId);
        }

        if (employee == null && !string.IsNullOrEmpty(user.Email))
        {
            employee = await _employeeRepository.GetByEmailAsync(user.Email);
        }

        var token = _jwtTokenGenerator.GenerateToken(
            user.Id,
            user.Username,
            user.Role,
            employee?.Id);

        return new LoginResponse
        {
            Token = token,
            Username = user.Username,
            FullName = employee != null ? $"{employee.FirstName} {employee.LastName}".Trim() : user.FullName,
            Role = user.Role,
            PositionName = employee?.Position?.Name ?? user.Role,
            EmployeeId = employee?.Id,
            RequiresPasswordChange = _passwordHasher.Verify(user.Username, user.PasswordHash)
        };
    }

    public async Task CreateUserAsync(CreateUserRequest request)
    {
        var user = new User
        {
            Username = request.Username,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            Email = request.Email,
            Role = request.Role,
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        if (!_passwordHasher.Verify(oldPassword, user.PasswordHash))
            throw new InvalidOperationException("Incorrect current password.");

        user.PasswordHash = _passwordHasher.Hash(newPassword);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task ForceResetPasswordAsync(int employeeId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
            throw new InvalidOperationException("Employee not found.");

        var username = $"EMP-SYNC-{employee.EmployeeCode}";
        var user = await _userRepository.GetByUsernameAsync(username) ?? await _userRepository.GetByUsernameAsync(employee.EmployeeCode) ?? await _userRepository.GetByUsernameAsync(employee.Email ?? "");
        
        if (user == null)
        {
            // Create user if not exists
            user = new User
            {
                Username = username,
                PasswordHash = _passwordHasher.Hash(username),
                FullName = $"{employee.FirstName} {employee.LastName}",
                Email = employee.Email ?? "",
                Role = "Employee",
                IsActive = true
            };
            await _userRepository.AddAsync(user);
        }
        else
        {
            user.Username = username;
            user.PasswordHash = _passwordHasher.Hash(username);
            _userRepository.Update(user);
        }
        
        await _userRepository.SaveChangesAsync();
    }

    public async Task GenerateLoginsAsync()
    {
        var employees = await _employeeRepository.GetAllAsync();
        foreach (var employee in employees)
        {
            if (!string.IsNullOrEmpty(employee.EmployeeCode))
            {
                var username = $"EMP-SYNC-{employee.EmployeeCode}";
                var existingUser = await _userRepository.GetByUsernameAsync(username);
                
                if (existingUser == null)
                {
                    var user = new User
                    {
                        Username = username,
                        PasswordHash = _passwordHasher.Hash(username),
                        FullName = $"{employee.FirstName} {employee.LastName}",
                        Email = employee.Email ?? "",
                        Role = "Employee",
                        IsActive = true
                    };
                    await _userRepository.AddAsync(user);
                }
                else
                {
                    existingUser.FullName = $"{employee.FirstName} {employee.LastName}";
                    existingUser.Email = employee.Email ?? "";
                    existingUser.IsActive = true;
                    _userRepository.Update(existingUser);
                }
            }
        }
        await _userRepository.SaveChangesAsync();
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null || !string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid username or email address.");
        }

        var token = Guid.NewGuid().ToString();
        user.ResetToken = token;
        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        var resetLink = $"http://localhost:5173/reset-password?email={user.Email}&token={token}";
        var body = $"Please click the following link to reset your password:\n\n{resetLink}\n\nIf you did not request this, please ignore this email.";
        
        await _emailService.SendEmailAsync(user.Email, "Password Reset", body);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var users = await _userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Email == request.Email && u.ResetToken == request.Token);
        
        if (user == null || user.ResetTokenExpiry == null || user.ResetTokenExpiry < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Invalid or expired reset token.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }
}
