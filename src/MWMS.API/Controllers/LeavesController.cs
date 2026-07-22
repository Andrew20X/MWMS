using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWMS.Application.DTOs;
using MWMS.Application.Interfaces;

namespace MWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeavesController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    public LeavesController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SubmitRequest(CreateLeaveRequestDto dto)
    {
        try
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin)
            {
                if (employeeIdClaim == null) return Unauthorized();
                dto.EmployeeId = int.Parse(employeeIdClaim);
            }

            var result = await _leaveService.SubmitRequestAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeeLeaves(int employeeId)
    {
        var leaves = await _leaveService.GetEmployeeLeavesAsync(employeeId);
        return Ok(leaves);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyLeaves()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (employeeIdClaim == null) return Unauthorized();

        var employeeId = int.Parse(employeeIdClaim);
        var leaves = await _leaveService.GetEmployeeLeavesAsync(employeeId);
        return Ok(leaves);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingLeaves()
    {
        var leaves = await _leaveService.GetPendingLeavesAsync();
        return Ok(leaves);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllLeaves()
    {
        var leaves = await _leaveService.GetAllLeavesAsync();
        return Ok(leaves);
    }

    /// <summary>
    /// Approve a leave request. Role-aware:
    /// - Manager → advances to PendingHRApproval.
    /// - Admin (HR) → final approval, deducts leave balance.
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ApproveLeave(int id, [FromBody] ActionRequestDto request)
    {
        try
        {
            var callerIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var callerId = int.TryParse(callerIdStr, out var cid) ? cid : 1;
            
            var callerRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Admin";
            var callerName = User.FindFirst("FullName")?.Value
                          ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                          ?? "Unknown";

            var success = await _leaveService.ApproveRequestAsync(id, callerId, callerName, callerRole, request.AdminMessage);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Reject a leave request. Both Manager and Admin can reject at their respective stages.
    /// </summary>
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RejectLeave(int id, [FromBody] ActionRequestDto request)
    {
        try
        {
            var callerIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var callerId = int.TryParse(callerIdStr, out var cid) ? cid : 1;
            
            var callerRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Admin";
            var callerName = User.FindFirst("FullName")?.Value
                          ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                          ?? "Unknown";

            var success = await _leaveService.RejectRequestAsync(id, callerId, callerName, callerRole, request.AdminMessage);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Returns approval history for a leave request.</summary>
    [HttpGet("{id}/history")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var history = await _leaveService.GetApprovalHistoryAsync(id, "Leave");
        return Ok(history);
    }

    // ─── Leave Balance Endpoints ──────────────────────────────────────────────

    /// <summary>Returns the leave balance for any employee (Admin/Manager only).</summary>
    [HttpGet("balance/{employeeId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetBalance(int employeeId, [FromQuery] int? year = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var balance = await _leaveService.GetLeaveBalanceAsync(employeeId, targetYear);
        return Ok(balance);
    }

    /// <summary>Returns the current employee's own leave balance.</summary>
    [HttpGet("balance/me")]
    [Authorize]
    public async Task<IActionResult> GetMyBalance([FromQuery] int? year = null)
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (employeeIdClaim == null) return Unauthorized();

        var employeeId = int.Parse(employeeIdClaim);
        var targetYear = year ?? DateTime.UtcNow.Year;
        var balance = await _leaveService.GetLeaveBalanceAsync(employeeId, targetYear);
        return Ok(balance);
    }

    /// <summary>Updates the leave balance for any employee (Admin/Manager only).</summary>
    [HttpPut("balance/{employeeId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateBalance(int employeeId, [FromBody] UpdateLeaveBalanceDto dto)
    {
        try
        {
            var updatedBalance = await _leaveService.UpdateLeaveBalanceAsync(employeeId, dto.Year, dto);
            return Ok(updatedBalance);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ─── Delete Endpoints ─────────────────────────────────────────────────────

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteLeave(int id)
    {
        var isAdmin = User.IsInRole("Admin");
        int employeeId = 0;

        if (!isAdmin)
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (employeeIdClaim == null) return Unauthorized();
            employeeId = int.Parse(employeeIdClaim);
        }

        var success = await _leaveService.DeleteRequestAsync(id, employeeId, isAdmin);
        if (!success) return Forbid();

        return NoContent();
    }

    [HttpDelete("all")]
    [Authorize]
    public async Task<IActionResult> DeleteAllLeaves()
    {
        var isAdmin = User.IsInRole("Admin");

        if (isAdmin)
        {
            await _leaveService.DeleteAllRequestsAsync();
        }
        else
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (employeeIdClaim == null) return Unauthorized();
            
            var employeeId = int.Parse(employeeIdClaim);
            await _leaveService.DeleteAllEmployeeRequestsAsync(employeeId);
        }

        return NoContent();
    }
}

public class ActionRequestDto
{
    public int ApproverId { get; set; }
    public string? AdminMessage { get; set; }
}
