using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;

namespace MWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OvertimeController : ControllerBase
{
    private readonly IOvertimeRepository _overtimeRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmailService _emailService;
    private readonly IGenericRepository<ApprovalHistory> _approvalHistoryRepository;

    public OvertimeController(
        IOvertimeRepository overtimeRepository,
        IEmployeeRepository employeeRepository,
        IEmailService emailService,
        IGenericRepository<ApprovalHistory> approvalHistoryRepository)
    {
        _overtimeRepository = overtimeRepository;
        _employeeRepository = employeeRepository;
        _emailService = emailService;
        _approvalHistoryRepository = approvalHistoryRepository;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyOvertimeRequests()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (employeeIdClaim == null) return Unauthorized();

        var employeeId = int.Parse(employeeIdClaim);
        var requests = await _overtimeRepository.GetByEmployeeAsync(employeeId);
        return Ok(requests);
    }

    [HttpPost("me")]
    public async Task<IActionResult> SubmitOvertimeRequest([FromBody] OvertimeRequest request)
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (employeeIdClaim == null) return Unauthorized();

        var employeeId = int.Parse(employeeIdClaim);
        request.EmployeeId = employeeId;
        request.Status = OvertimeRequest.StatusPendingManager; // Starts at Manager stage
        request.CreatedAt = DateTime.UtcNow;

        await _overtimeRepository.AddAsync(request);
        await _overtimeRepository.SaveChangesAsync();

        return Ok(request);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAllOvertimeRequests()
    {
        var requests = await _overtimeRepository.GetAllAsync();
        return Ok(requests);
    }

    /// <summary>
    /// Approve an overtime request. Role-aware:
    /// - Manager → advances to PendingHRApproval.
    /// - Admin (HR) → fully approved.
    /// </summary>
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ApproveOvertime(int id, [FromBody] string? adminNote)
    {
        var request = await _overtimeRepository.GetByIdAsync(id);
        if (request == null) return NotFound();

        var callerRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Admin";
        var callerName = User.FindFirst("FullName")?.Value
                      ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                      ?? "Unknown";
        var callerIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0";
        int callerId = int.TryParse(callerIdClaim, out var cid) ? cid : 0;

        string decision;

        if (callerRole == "Manager")
        {
            if (request.Status != OvertimeRequest.StatusPendingManager)
                return BadRequest(new { error = "This request is not awaiting Manager approval." });

            request.Status = OvertimeRequest.StatusPendingHR;
            decision = "Approved by Manager";
        }
        else // Admin (HR)
        {
            if (request.Status != OvertimeRequest.StatusPendingHR)
                return BadRequest(new { error = "This request must be approved by a Manager first." });

            request.Status = OvertimeRequest.StatusApproved;
            decision = "Approved by HR";
        }

        request.AdminNote = adminNote;
        request.UpdatedAt = DateTime.UtcNow;
        _overtimeRepository.Update(request);

        // Record approval history
        var history = new ApprovalHistory
        {
            RequestType = "Overtime",
            RequestId = id,
            ApproverId = callerId,
            ApproverName = callerName,
            ApproverRole = callerRole,
            Decision = decision,
            Comment = adminNote,
            DecisionAt = DateTime.UtcNow
        };
        await _approvalHistoryRepository.AddAsync(history);
        await _overtimeRepository.SaveChangesAsync();

        // Notify employee
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
        if (employee != null && !string.IsNullOrEmpty(employee.Email))
        {
            var isFullyApproved = request.Status == OvertimeRequest.StatusApproved;
            var subject = isFullyApproved ? "Overtime Approved" : "Overtime – Manager Approved (Pending HR)";
            var body = isFullyApproved
                ? $"Hello {employee.FirstName},\n\nYour Overtime request for {request.Date:yyyy-MM-dd} from {request.StartTime} to {request.EndTime} has been fully approved.\nNote: {adminNote ?? "None"}"
                : $"Hello {employee.FirstName},\n\nYour Overtime request for {request.Date:yyyy-MM-dd} has been approved by your Manager and is now pending HR final approval.\nNote: {adminNote ?? "None"}";

            _ = Task.Run(async () =>
            {
                try { await _emailService.SendEmailAsync(employee.Email!, subject, body); } catch { }
            });
        }

        return Ok(request);
    }

    /// <summary>Reject an overtime request at any stage. Both Manager and Admin can reject.</summary>
    [HttpPut("{id}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RejectOvertime(int id, [FromBody] string? adminNote)
    {
        var request = await _overtimeRepository.GetByIdAsync(id);
        if (request == null) return NotFound();

        var callerRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Admin";
        var callerName = User.FindFirst("FullName")?.Value
                      ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                      ?? "Unknown";
        var callerIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0";
        int callerId = int.TryParse(callerIdClaim, out var cid) ? cid : 0;

        // Validate stage access
        if (callerRole == "Manager" && request.Status != OvertimeRequest.StatusPendingManager)
            return BadRequest(new { error = "This request is not awaiting Manager approval." });

        if (callerRole == "Admin" &&
            request.Status != OvertimeRequest.StatusPendingHR &&
            request.Status != OvertimeRequest.StatusPendingManager)
            return BadRequest(new { error = "This request cannot be rejected at its current stage." });

        request.Status = OvertimeRequest.StatusRejected;
        request.AdminNote = adminNote;
        request.UpdatedAt = DateTime.UtcNow;
        _overtimeRepository.Update(request);

        // Record rejection history
        var history = new ApprovalHistory
        {
            RequestType = "Overtime",
            RequestId = id,
            ApproverId = callerId,
            ApproverName = callerName,
            ApproverRole = callerRole,
            Decision = "Rejected",
            Comment = adminNote,
            DecisionAt = DateTime.UtcNow
        };
        await _approvalHistoryRepository.AddAsync(history);
        await _overtimeRepository.SaveChangesAsync();

        // Notify employee
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
        if (employee != null && !string.IsNullOrEmpty(employee.Email))
        {
            var subject = "Overtime Rejected";
            var body = $"Hello {employee.FirstName},\n\nYour Overtime request for {request.Date:yyyy-MM-dd} from {request.StartTime} to {request.EndTime} has been rejected by {callerRole}.\nNote: {adminNote ?? "None"}";
            _ = Task.Run(async () =>
            {
                try { await _emailService.SendEmailAsync(employee.Email!, subject, body); } catch { }
            });
        }

        return Ok(request);
    }

    /// <summary>Returns approval history for an overtime request.</summary>
    [HttpGet("{id}/history")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var allHistory = await _approvalHistoryRepository.GetAllAsync();
        var history = allHistory
            .Where(h => h.RequestId == id && h.RequestType == "Overtime" && !h.IsDeleted)
            .OrderBy(h => h.DecisionAt)
            .Select(h => new
            {
                h.Id,
                h.ApproverName,
                h.ApproverRole,
                h.Decision,
                h.Comment,
                h.DecisionAt
            });
        return Ok(history);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOvertime(int id)
    {
        var request = await _overtimeRepository.GetByIdAsync(id);
        if (request == null) return NotFound();

        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (roleClaim != "Admin" && roleClaim != "Manager")
        {
            if (employeeIdClaim == null || int.Parse(employeeIdClaim) != request.EmployeeId)
                return Forbid();
        }

        _overtimeRepository.Delete(request);
        await _overtimeRepository.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("all")]
    public async Task<IActionResult> DeleteAllOvertimes()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (roleClaim == "Admin" || roleClaim == "Manager")
        {
            var requests = await _overtimeRepository.GetAllAsync();
            foreach (var req in requests)
            {
                _overtimeRepository.Delete(req);
            }
            await _overtimeRepository.SaveChangesAsync();
        }
        else
        {
            if (employeeIdClaim == null) return Unauthorized();
            var employeeId = int.Parse(employeeIdClaim);
            var requests = await _overtimeRepository.GetByEmployeeAsync(employeeId);
            foreach (var req in requests)
            {
                _overtimeRepository.Delete(req);
            }
            await _overtimeRepository.SaveChangesAsync();
        }

        return Ok();
    }
}
