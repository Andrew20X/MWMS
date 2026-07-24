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

        // Find unresolved absences past the deadline where there is no pending leave request linked
        var expiredAbsences = await dbContext.Attendances
            .Include(a => a.Employee)
            .Where(a => a.Status == AttendanceStatus.Absent &&
                        a.AbsenceResolutionStatus == AbsenceResolutionStatus.PendingResolution &&
                        a.DeadlineForLeaveRequest < now &&
                        !dbContext.LeaveRequests.Any(lr => lr.LinkedAttendanceId == a.Id && 
                                                          (lr.Status == LeaveStatus.PendingManagerApproval || lr.Status == LeaveStatus.PendingHRApproval)))
            .ToListAsync(stoppingToken);

        foreach (var absence in expiredAbsences)
        {
            absence.AbsenceResolutionStatus = AbsenceResolutionStatus.DeductionApplied;

            // Simple deduction logic: 1 day's pay. 
            // In a real app, this might calculate based on monthly salary or hourly rate.
            // For now, assuming a fixed rate of $150 or pulling from a config/employee record.
            decimal standardDailyRate = 150.0m;

            var deduction = new SalaryDeduction
            {
                EmployeeId = absence.EmployeeId,
                RelatedAttendanceId = absence.Id,
                DeductionAmount = standardDailyRate,
                Reason = $"AWOL: No leave request submitted within 2 days for absence on {absence.Date}",
                AppliedOnDate = now,
                Status = PayrollStatus.PendingPayroll
            };

            dbContext.SalaryDeductions.Add(deduction);

            // Notify Employee
            var subject = "Notice: Salary Deduction Applied";
            var body = $"<p>Dear {absence.Employee.FirstName} {absence.Employee.LastName},</p><p>A salary deduction of ${standardDailyRate} has been applied to your upcoming payroll due to an unresolved absence on {absence.Date}.</p>";
            
            if (!string.IsNullOrEmpty(absence.Employee.Email))
            {
                try
                {
                    await emailService.SendEmailAsync(absence.Employee.Email, subject, body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send deduction notice to {Email}", absence.Employee.Email);
                }
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
        _logger.LogInformation("Deduction enforcement completed. Processed {Count} absences.", expiredAbsences.Count);
    }
}
