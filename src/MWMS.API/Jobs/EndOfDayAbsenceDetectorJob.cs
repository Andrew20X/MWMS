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
                // Run absence detection immediately
                await RunAbsenceDetectionAsync(stoppingToken);
                
                // Sleep for 2 hours before checking again
                await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
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
        var startDate = today.AddDays(-7);
        var currentHour = DateTime.Now.Hour;

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        // Find all active employees who are not deleted and have a valid fingerprint ID
        var employees = await dbContext.Employees.Where(e => e.IsActive && !e.IsDeleted && e.DeviceUserId > 0).ToListAsync(stoppingToken);

        foreach (var employee in employees)
        {
            for (var date = startDate; date <= today; date = date.AddDays(1))
            {
                // Skip checking today until 11 PM (23:00)
                if (date == today && currentHour < 23)
                    continue;

                // Skip weekends (Friday and Saturday)
                if (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday)
                    continue;

                // Skip dates before the employee was hired
                if (date < employee.HireDate)
                    continue;

                // Check if there is an attendance record for this date
                var hasAttendance = await dbContext.Attendances
                    .AnyAsync(a => a.EmployeeId == employee.Id && a.Date == date, stoppingToken);

                if (!hasAttendance)
                {
                    // Check if they have an approved leave for this date
                    var hasLeave = await dbContext.LeaveRequests
                        .AnyAsync(l => l.EmployeeId == employee.Id && 
                                       l.StartDate <= date && l.EndDate >= date &&
                                       l.Status == LeaveStatus.Approved, stoppingToken);

                    if (!hasLeave)
                    {
                        // Employee is absent without leave
                        var absenceRecord = new Attendance
                        {
                            EmployeeId = employee.Id,
                            Date = date,
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
                            Reason = $"AWOL: Unexcused absence on {date}",
                            AppliedOnDate = DateTime.Now,
                            Status = PayrollStatus.PendingPayroll
                        };
                        dbContext.SalaryDeductions.Add(deduction);
                    }
                }
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
        _logger.LogInformation("Absence detection completed up to {Date}.", today);
    }
}
