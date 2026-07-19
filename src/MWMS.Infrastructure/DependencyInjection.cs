using Microsoft.Extensions.DependencyInjection;
using MWMS.Application.Interfaces;
using MWMS.Application.Services;
using MWMS.Infrastructure.Services;

namespace MWMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAttendanceEngineService, AttendanceEngineService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ILeaveService, LeaveService>();
        services.AddScoped<IPositionService, PositionService>();
        return services;
    }
}
