using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MWMS.Application.DTOs.Attendance;
using MWMS.Application.Interfaces;

namespace MWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceEngineController : ControllerBase
{
    private readonly IAttendanceEngineService _engineService;

    public AttendanceEngineController(IAttendanceEngineService engineService)
    {
        _engineService = engineService;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessRawLogs([FromBody] List<RawPunchDto> logs)
    {
        if (logs == null || !logs.Any())
        {
            return BadRequest("No logs provided.");
        }

        try
        {
            await _engineService.ProcessRawLogsAsync(logs);
            return Ok(new { message = "Logs processed successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("simulate-awol")]
    // Removed [Authorize] so it can be called from the browser easily
    public async Task<IActionResult> SimulateAwol([FromServices] MWMS.Persistence.Context.AppDbContext dbContext, [FromQuery] int employeeId = 1)
    {
        // Use the logged-in user if available, otherwise fallback to the query parameter (default 1)
        var employeeIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(employeeIdClaim))
        {
            employeeId = int.Parse(employeeIdClaim);
        }
        else
        {
            // Fallback to the first employee in the database if no user is logged in
            var firstEmployee = await dbContext.Employees.FirstOrDefaultAsync();
            if (firstEmployee == null) return BadRequest("No employees exist in the database.");
            employeeId = firstEmployee.Id;
        }
        
        var absenceRecord = new MWMS.Domain.Entities.Attendance
        {
            EmployeeId = employeeId,
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-3)), // 3 days ago to simulate expired deadline
            Status = MWMS.Domain.Enums.AttendanceStatus.Absent,
            IsUnexcused = true,
            AbsenceResolutionStatus = MWMS.Domain.Enums.AbsenceResolutionStatus.PendingResolution,
            DeadlineForLeaveRequest = DateTime.Now.AddDays(-1) // Expired yesterday
        };

        dbContext.Attendances.Add(absenceRecord);
        await dbContext.SaveChangesAsync();

        return Ok(new { message = $"Simulated an unexcused absence for Employee ID {employeeId}. Check the dashboard!" });
    }

    [HttpGet("simulate-deduction")]
    // Removed [Authorize] so it can be called from the browser easily
    public async Task<IActionResult> SimulateDeduction(
        [FromServices] MWMS.Persistence.Context.AppDbContext dbContext,
        [FromServices] IEmailService emailService,
        [FromServices] Microsoft.Extensions.Logging.ILogger<AttendanceEngineController> logger)
    {
        var now = DateTime.Now;
        var expiredAbsences = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            System.Linq.Queryable.Where(dbContext.Attendances.Include(a => a.Employee), a => 
                a.Status == MWMS.Domain.Enums.AttendanceStatus.Absent &&
                a.AbsenceResolutionStatus == MWMS.Domain.Enums.AbsenceResolutionStatus.PendingResolution &&
                a.DeadlineForLeaveRequest < now &&
                !dbContext.LeaveRequests.Any(lr => lr.LinkedAttendanceId == a.Id && 
                                                  (lr.Status == MWMS.Domain.Enums.LeaveStatus.PendingManagerApproval || lr.Status == MWMS.Domain.Enums.LeaveStatus.PendingHRApproval)))
        );

        int count = 0;
        foreach (var absence in expiredAbsences)
        {
            absence.AbsenceResolutionStatus = MWMS.Domain.Enums.AbsenceResolutionStatus.DeductionApplied;
            decimal standardDailyRate = 150.0m;

            var deduction = new MWMS.Domain.Entities.SalaryDeduction
            {
                EmployeeId = absence.EmployeeId,
                RelatedAttendanceId = absence.Id,
                DeductionAmount = standardDailyRate,
                Reason = $"AWOL: No leave request submitted within 2 days for absence on {absence.Date}",
                AppliedOnDate = now,
                Status = MWMS.Domain.Enums.PayrollStatus.PendingPayroll
            };

            dbContext.SalaryDeductions.Add(deduction);
            count++;

            var subject = "Notice: Salary Deduction Applied";
            var body = $"<p>Dear {absence.Employee.FirstName} {absence.Employee.LastName},</p><p>A salary deduction of ${standardDailyRate} has been applied to your upcoming payroll due to an unresolved absence on {absence.Date}.</p>";
            
            if (!string.IsNullOrEmpty(absence.Employee.Email))
            {
                try { await emailService.SendEmailAsync(absence.Employee.Email, subject, body); }
                catch (Exception ex) { logger.LogError(ex, "Failed to send deduction notice to {Email}", absence.Employee.Email); }
            }
        }

        await dbContext.SaveChangesAsync();
        return Ok(new { message = $"Deduction simulation complete. Processed {count} expired absences." });
    }
}
