using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MWMS.Domain.Enums;
using MWMS.Persistence.Context;
using System.Security.Claims;

namespace MWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeductionsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly MWMS.Application.Interfaces.IEmailService _emailService;

    public DeductionsController(AppDbContext dbContext, MWMS.Application.Interfaces.IEmailService emailService)
    {
        _dbContext = dbContext;
        _emailService = emailService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin, HR")]
    public async Task<IActionResult> GetAllDeductions()
    {
        var deductions = await _dbContext.SalaryDeductions
            .Include(d => d.Employee)
            .Include(d => d.RelatedAttendance)
            .OrderByDescending(d => d.AppliedOnDate)
            .Select(d => new
            {
                d.Id,
                d.EmployeeId,
                EmployeeName = d.Employee.FirstName + " " + d.Employee.LastName,
                d.RelatedAttendanceId,
                AttendanceDate = d.RelatedAttendance.Date,
                d.DeductionAmount,
                d.Reason,
                d.AppliedOnDate,
                Status = d.Status.ToString()
            })
            .ToListAsync();

        return Ok(deductions);
    }

    [HttpGet("my-deductions")]
    public async Task<IActionResult> GetMyDeductions()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var employeeId))
        {
            return Unauthorized();
        }

        var deductions = await _dbContext.SalaryDeductions
            .Include(d => d.RelatedAttendance)
            .Where(d => d.EmployeeId == employeeId)
            .OrderByDescending(d => d.AppliedOnDate)
            .Select(d => new
            {
                d.Id,
                d.RelatedAttendanceId,
                AttendanceDate = d.RelatedAttendance.Date,
                d.DeductionAmount,
                d.Reason,
                d.AppliedOnDate,
                Status = d.Status.ToString()
            })
            .ToListAsync();

        return Ok(deductions);
    }

    public class WaiveDeductionRequest
    {
        public string? Reason { get; set; }
    }

    [HttpPost("{id}/waive")]
    [Authorize(Roles = "Admin, HR")]
    public async Task<IActionResult> WaiveDeduction(int id, [FromBody] WaiveDeductionRequest request)
    {
        var deduction = await _dbContext.SalaryDeductions
            .Include(d => d.Employee)
            .Include(d => d.RelatedAttendance)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deduction == null)
        {
            return NotFound("Deduction not found.");
        }

        if (deduction.Status == PayrollStatus.Processed)
        {
            return BadRequest("Cannot waive a deduction that has already been processed in payroll.");
        }

        deduction.Status = PayrollStatus.Waived;
        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            deduction.RejectionReason = request.Reason;
        }
        
        // Optionally update the attendance resolution status if desired
        if (deduction.RelatedAttendance != null && deduction.RelatedAttendance.AbsenceResolutionStatus == AbsenceResolutionStatus.DeductionApplied)
        {
            deduction.RelatedAttendance.AbsenceResolutionStatus = AbsenceResolutionStatus.Waived;
        }

        await _dbContext.SaveChangesAsync();

        if (deduction.Employee != null && !string.IsNullOrEmpty(deduction.Employee.Email))
        {
            var subject = "Notice: Salary Deduction Waived";
            var body = $"<p>Dear {deduction.Employee.FirstName},</p><p>Your pending salary deduction for the absence on {deduction.RelatedAttendance?.Date.ToShortDateString()} has been <strong>rejected/waived</strong> by the administration. No amount will be deducted.</p>";
            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                body += $"<p><strong>Reason/Comment:</strong> {request.Reason}</p>";
            }
            try { await _emailService.SendEmailAsync(deduction.Employee.Email, subject, body); } catch { }
        }

        return Ok(new { message = "Deduction waived successfully." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin, HR")]
    public async Task<IActionResult> DeleteDeduction(int id)
    {
        var deduction = await _dbContext.SalaryDeductions.FindAsync(id);
        if (deduction == null)
        {
            return NotFound("Deduction not found.");
        }

        if (deduction.Status == PayrollStatus.Processed)
        {
            return BadRequest("Cannot delete a deduction that has already been processed in payroll.");
        }

        _dbContext.SalaryDeductions.Remove(deduction);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Deduction deleted successfully." });
    }

    [HttpDelete("all")]
    [Authorize(Roles = "Admin, HR")]
    public async Task<IActionResult> DeleteAllDeductions()
    {
        var deductions = await _dbContext.SalaryDeductions.ToListAsync();
        
        if (!deductions.Any())
        {
            return Ok(new { message = "No deductions to delete." });
        }

        _dbContext.SalaryDeductions.RemoveRange(deductions);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "All deductions have been deleted successfully." });
    }

    public class UpdateDeductionRequest
    {
        public decimal DeductionAmount { get; set; }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin, HR")]
    public async Task<IActionResult> UpdateDeduction(int id, [FromBody] UpdateDeductionRequest request)
    {
        var deduction = await _dbContext.SalaryDeductions.FindAsync(id);
        if (deduction == null)
        {
            return NotFound("Deduction not found.");
        }

        if (deduction.Status == PayrollStatus.Processed)
        {
            return BadRequest("Cannot edit a deduction that has already been processed in payroll.");
        }

        deduction.DeductionAmount = request.DeductionAmount;
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Deduction amount updated successfully." });
    }

    public class RejectExceptionRequest
    {
        public string? RejectionReason { get; set; }
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin, HR")]
    public async Task<IActionResult> RejectException(int id, [FromBody] RejectExceptionRequest request)
    {
        var deduction = await _dbContext.SalaryDeductions
            .Include(d => d.Employee)
            .Include(d => d.RelatedAttendance)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deduction == null)
        {
            return NotFound("Deduction not found.");
        }

        if (deduction.Status == PayrollStatus.Processed)
        {
            return BadRequest("Cannot reject an exception that has already been processed in payroll.");
        }

        deduction.Status = PayrollStatus.Rejected;
        deduction.RejectionDate = DateTime.Now;
        deduction.RejectionReason = request.RejectionReason;

        var callerName = User.FindFirst("FullName")?.Value
                      ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                      ?? "Admin";

        // The admin message is already stored in request.RejectionReason and rejection history can be recorded elsewhere.
        // We no longer append this to deduction.Reason to preserve the original reason.

        if (deduction.RelatedAttendance != null && deduction.RelatedAttendance.AbsenceResolutionStatus == AbsenceResolutionStatus.PendingResolution)
        {
            deduction.RelatedAttendance.AbsenceResolutionStatus = AbsenceResolutionStatus.ExceptionRejected;
        }

        await _dbContext.SaveChangesAsync();

        if (deduction.Employee != null && !string.IsNullOrEmpty(deduction.Employee.Email))
        {
            var subject = "Notice: Salary Deduction Approved";
            var body = $"<p>Dear {deduction.Employee.FirstName},</p><p>Your pending salary deduction for the absence on {deduction.RelatedAttendance?.Date.ToShortDateString()} has been <strong>approved</strong> by the administration and will be applied to your upcoming payroll.</p>";
            if (!string.IsNullOrEmpty(request.RejectionReason))
            {
                body += $"<p><strong>Reason/Comment:</strong> {request.RejectionReason}</p>";
            }
            try { await _emailService.SendEmailAsync(deduction.Employee.Email, subject, body); } catch { }
        }

        return Ok(new { message = "Exception rejected successfully." });
    }
}
