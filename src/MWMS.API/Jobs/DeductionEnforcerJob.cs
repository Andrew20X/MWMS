using Microsoft.EntityFrameworkCore;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Domain.Enums;
using MWMS.Persistence.Context;

namespace MWMS.API.Jobs;

public class DeductionEnforcerJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeductionEnforcerJob> _logger;

    public DeductionEnforcerJob(IServiceProvider serviceProvider, ILogger<DeductionEnforcerJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                // Run at 1:00 AM daily
                if (now.Hour == 1)
                {
                    await EnforceDeductionsAsync(stoppingToken);
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                else
                {
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown
                break;
            }
            catch (Exception ex)
            {
                try { _logger.LogError(ex, "Error occurred executing deduction enforcer job."); } catch { }
                try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); } catch { break; }
            }
        }
    }

    private async Task EnforceDeductionsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var now = DateTime.Now;

        // Deduction enforcement is now handled immediately on the same day by EndOfDayAbsenceDetectorJob.
        // This job is kept for future delayed enforcement rules if needed.
        await Task.CompletedTask;
    }
}
