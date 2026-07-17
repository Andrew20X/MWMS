namespace MWMS.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(int userId, string username, string role, int? employeeId = null);
}