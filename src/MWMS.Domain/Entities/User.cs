using MWMS.Domain.Common;

namespace MWMS.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = "Employee";

    public bool IsActive { get; set; } = true;

    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpiry { get; set; }
}