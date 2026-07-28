using Microsoft.EntityFrameworkCore;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Domain.Enums;
using MWMS.Persistence.Context;

namespace MWMS.API.Jobs;

public class EndOfDayAbsenceDetectorJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EndOfDayAbsenceDetectorJob> _logger;

    public EndOfDayAbsenceDetectorJob(IServiceProvider serviceProvider, ILogger<EndOfDayAbsenceDetectorJob> logger)
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
                // In a real app, you'd use a robust scheduler like Quartz.NET to run at exactly 11:59 PM.
                // For simplicity, we delay until the end of the day or run periodically.
                // We'll simulate a daily run by checking every hour (or use a daily delay).
                
                var now = DateTime.Now;
                // If it's near midnight, run logic (simulated for simplicity, we'll just run it)
                if (now.Hour == 23)
                {
                    await RunAbsenceDetectionAsync(stoppingToken);
                    // Sleep for 24 hours to prevent running multiple times
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                else
                {
                    // Check every 10 minutes to see if it's 23:00
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
                try { _logger.LogError(ex, "Error occurred executing absence detector job."); } catch { }
                try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); } catch { break; }
            }
        }
    }

    private async Task RunAbsenceDetectionAsync(CancellationToken stoppingToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        
        // Skip weekends (Friday and Saturday)
        if (today.DayOfWeek == DayOfWeek.Friday || today.DayOfWeek == DayOfWeek.Saturday)
        {
            _logger.LogInformation("Absence detection skipped for weekend ({DayOfWeek}).", today.DayOfWeek);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        // Find all active employees
        var employees = await dbContext.Employees.Where(e => e.IsActive).ToListAsync(stoppingToken);

        foreach (var employee in employees)
        {
            // Check if there is an attendance record for today
            var hasAttendance = await dbContext.Attendances
                .AnyAsync(a => a.EmployeeId == employee.Id && a.Date == today, stoppingToken);

            if (!hasAttendance)
            {
                // Check if they have an approved leave for today
                var hasLeave = await dbContext.LeaveRequests
                    .AnyAsync(l => l.EmployeeId == employee.Id && 
                                   l.StartDate <= today && l.EndDate >= today &&
                                   l.Status == LeaveStatus.Approved, stoppingToken);

                if (!hasLeave)
                {
                    // Employee is absent without leave
                    var absenceRecord = new Attendance
                    {
                        EmployeeId = employee.Id,
                        Date = today,
                        Status = AttendanceStatus.Absent,
                        IsUnexcused = true,
                        AbsenceResolutionStatus = AbsenceResolutionStatus.PendingResolution,
                        DeadlineForLeaveRequest = DateTime.Now.AddDays(2)
                    };

                    dbContext.Attendances.Add(absenceRecord);

                    // Create the deduction immediately on the same day
                    var deduction = new SalaryDeduction
                    {
                        EmployeeId = employee.Id,
                        RelatedAttendance = absenceRecord,
                        DeductionAmount = 1.0m, // 1 day deduction
                        Reason = $"AWOL: Unexcused absence on {today}",
                        AppliedOnDate = DateTime.Now,
                        Status = PayrollStatus.PendingPayroll
                    };
                    dbContext.SalaryDeductions.Add(deduction);

                    // Create Warning Announcement
                    var announcement = new Announcement
                    {
                        Title = "Warning: Unexcused Absence",
                        Content = $"You were absent on {today} without a leave request. A salary deduction is pending admin review.",
                        Type = "Notice",
                        TargetEmployeeId = employee.Id
                    };
                    dbContext.Announcements.Add(announcement);

                    // Send email warning
                    var subject = "Action Required: Pending Salary Deduction";
                    var body = $"<p>Dear {employee.FirstName} {employee.LastName},</p><p>You were marked absent today ({today}) and did not submit a leave request. A salary deduction is currently pending admin review.</p>";
                    
                    if (!string.IsNullOrEmpty(employee.Email))
                    {
                        try 
                        {
                            await emailService.SendEmailAsync(employee.Email, subject, body);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send absence warning to {Email}", employee.Email);
                        }
                    }
                }
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
        _logger.LogInformation("Absence detection completed for {Date}.", today);
    }
}
