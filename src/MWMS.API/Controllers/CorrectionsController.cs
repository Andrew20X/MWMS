using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Domain.Enums;

namespace MWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CorrectionsController : ControllerBase
{
    private readonly ICorrectionRepository _correctionRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmailService _emailService;

    public CorrectionsController(
        ICorrectionRepository correctionRepository, 
        IAttendanceRepository attendanceRepository, 
        IEmployeeRepository employeeRepository,
        IEmailService emailService)
    {
        _correctionRepository = correctionRepository;
        _attendanceRepository = attendanceRepository;
        _employeeRepository = employeeRepository;
        _emailService = emailService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyCorrections()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (employeeIdClaim == null) return Unauthorized();

        var employeeId = int.Parse(employeeIdClaim);
        var requests = await _correctionRepository.GetByEmployeeIdAsync(employeeId);
        return Ok(requests);
    }

    [HttpPost]
    public async Task<IActionResult> RequestCorrection([FromBody] CorrectionRequest request)
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (employeeIdClaim == null) return Unauthorized();

        request.EmployeeId = int.Parse(employeeIdClaim);
        request.Status = "Pending";
        request.CreatedAt = DateTime.UtcNow;
        
        await _correctionRepository.AddAsync(request);
        await _correctionRepository.SaveChangesAsync();
        return Ok(request);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingCorrections()
    {
        var requests = await _correctionRepository.GetPendingRequestsAsync();
        return Ok(requests);
    }

    public class CorrectionActionDto
    {
        public string? Note { get; set; }
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveCorrection(int id, [FromBody] CorrectionActionDto? actionDto = null)
    {
        var request = await _correctionRepository.GetByIdAsync(id);
        if (request == null) return NotFound();

        request.Status = "Approved";
        request.AdminNote = actionDto?.Note;
        request.UpdatedAt = DateTime.UtcNow;
        _correctionRepository.Update(request);
        await _correctionRepository.SaveChangesAsync();

        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
        var shift = employee?.Shift;

        // Update actual attendance
        var attendance = await _attendanceRepository.GetByEmployeeAndDateAsync(request.EmployeeId, request.Date);
        bool isNew = false;
        
        if (attendance == null)
        {
            isNew = true;
            attendance = new Attendance
            {
                EmployeeId = request.EmployeeId,
                Date = request.Date,
                Status = AttendanceStatus.Present
            };
        }

        if (request.RequestedCheckIn.HasValue) attendance.CheckIn = request.RequestedCheckIn;
        if (request.RequestedCheckOut.HasValue) attendance.CheckOut = request.RequestedCheckOut;
        attendance.UpdatedAt = DateTime.UtcNow;

        if (shift != null && attendance.CheckIn.HasValue)
        {
            // Since this is an APPROVED correction, we excuse the lateness/early leave
            attendance.Status = AttendanceStatus.Present;
            attendance.LateMinutes = 0;
            attendance.EarlyLeaveMinutes = 0;

            if (attendance.CheckOut.HasValue)
            {
                attendance.WorkedHours = (attendance.CheckOut.Value.ToTimeSpan() - attendance.CheckIn.Value.ToTimeSpan()).TotalHours;

                if (attendance.CheckOut.Value > shift.EndTime)
                {
                    attendance.OvertimeMinutes = (int)(attendance.CheckOut.Value.ToTimeSpan() - shift.EndTime.ToTimeSpan()).TotalMinutes;
                }
                else
                {
                    attendance.OvertimeMinutes = 0;
                }
            }
        }

        if (isNew)
        {
            await _attendanceRepository.AddAsync(attendance);
        }
        else
        {
            _attendanceRepository.Update(attendance);
        }
        
        await _attendanceRepository.SaveChangesAsync();

        if (employee != null && !string.IsNullOrEmpty(employee.Email))
        {
            var subject = "Attendance Correction Approved";
            var body = $"Hello {employee.FirstName},\n\nYour attendance correction request for {request.Date:yyyy-MM-dd} has been approved.\nAdmin Note: {request.AdminNote ?? "None"}";
            await _emailService.SendEmailAsync(employee.Email, subject, body);
        }

        return Ok(request);
    }

    [HttpPut("{id}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectCorrection(int id, [FromBody] CorrectionActionDto? actionDto = null)
    {
        var request = await _correctionRepository.GetByIdAsync(id);
        if (request == null) return NotFound();

        request.Status = "Rejected";
        request.AdminNote = actionDto?.Note;
        request.UpdatedAt = DateTime.UtcNow;
        _correctionRepository.Update(request);
        await _correctionRepository.SaveChangesAsync();

        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
        if (employee != null && !string.IsNullOrEmpty(employee.Email))
        {
            var subject = "Attendance Correction Rejected";
            var body = $"Hello {employee.FirstName},\n\nYour attendance correction request for {request.Date:yyyy-MM-dd} has been rejected.\nAdmin Note: {request.AdminNote ?? "None"}";
            await _emailService.SendEmailAsync(employee.Email, subject, body);
        }

        return Ok(request);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCorrection(int id)
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (employeeIdClaim == null) return Unauthorized();

        var employeeId = int.Parse(employeeIdClaim);
        var request = await _correctionRepository.GetByIdAsync(id);

        if (request == null) return NotFound();

        if (request.EmployeeId != employeeId && !User.IsInRole("Admin"))
            return Forbid();

        _correctionRepository.Delete(request);
        await _correctionRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("all")]
    public async Task<IActionResult> DeleteAllMyCorrections()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (employeeIdClaim == null) return Unauthorized();

        var employeeId = int.Parse(employeeIdClaim);
        var requests = await _correctionRepository.GetByEmployeeIdAsync(employeeId);

        foreach (var req in requests)
        {
            _correctionRepository.Delete(req);
        }
        await _correctionRepository.SaveChangesAsync();

        return NoContent();
    }
}
