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
public class AnnouncementsController : ControllerBase
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IEmailService _emailService;
    private readonly IEmployeeRepository _employeeRepository;

    public AnnouncementsController(
        IAnnouncementRepository announcementRepository,
        IEmailService emailService,
        IEmployeeRepository employeeRepository)
    {
        _announcementRepository = announcementRepository;
        _emailService = emailService;
        _employeeRepository = employeeRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveAnnouncements()
    {
        var announcements = await _announcementRepository.GetActiveAnnouncementsAsync();
        
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (employeeIdClaim != null)
        {
            var myId = int.Parse(employeeIdClaim);
            // Only return global announcements or announcements targeted to this employee
            announcements = announcements.Where(a => a.TargetEmployeeId == null || a.TargetEmployeeId == myId).ToList();
        }
        
        return Ok(announcements);
    }

    /// <summary>
    /// Creates an announcement and sends a notification email.
    /// If <c>targetEmployeeId</c> is provided, the email is sent only to that employee.
    /// Otherwise it is broadcast to all active employees with a valid email address.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementRequest body)
    {
        if (string.IsNullOrWhiteSpace(body.Title) || string.IsNullOrWhiteSpace(body.Content))
            return BadRequest(new { error = "Title and Content are required." });

        var announcement = new Announcement
        {
            Title = body.Title,
            Content = body.Content,
            Type = body.Type ?? "Notice",
            TargetEmployeeId = body.TargetEmployeeId,
            CreatedAt = DateTime.UtcNow
        };

        await _announcementRepository.AddAsync(announcement);
        await _announcementRepository.SaveChangesAsync();

        // Determine email recipients
        if (body.TargetEmployeeId.HasValue)
        {
            // Send to a specific employee only
            var targetEmployee = await _employeeRepository.GetByIdAsync(body.TargetEmployeeId.Value);
            if (targetEmployee == null || string.IsNullOrEmpty(targetEmployee.Email))
                return BadRequest(new { error = "Selected employee does not have a valid email address." });

            if (targetEmployee.Email == "(No Email)")
                return BadRequest(new { error = $"Employee {targetEmployee.FirstName} {targetEmployee.LastName} has no email address on file." });

            var subject = $"Notice: {announcement.Title}";
            var emailBody = $"Hello {targetEmployee.FirstName},\n\nYou have received a notice:\n\n{announcement.Content}";
            _ = Task.Run(async () =>
            {
                try { await _emailService.SendEmailAsync(targetEmployee.Email!, subject, emailBody); } catch { }
            });
        }
        else
        {
            // Broadcast to all active employees
            var employees = await _employeeRepository.GetAllAsync();
            var employeesToEmail = employees
                .Where(e => e.IsActive && !e.IsDeleted && !string.IsNullOrEmpty(e.Email) && e.Email != "(No Email)")
                .ToList();

            _ = Task.Run(async () =>
            {
                foreach (var employee in employeesToEmail)
                {
                    var subject = $"New Announcement: {announcement.Title}";
                    var emailBody = $"Hello {employee.FirstName},\n\nA new announcement has been posted:\n\n{announcement.Content}";
                    try { await _emailService.SendEmailAsync(employee.Email!, subject, emailBody); } catch { /* ignore */ }
                }
            });
        }

        return Ok(announcement);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAnnouncement(int id)
    {
        var announcement = await _announcementRepository.GetByIdAsync(id);
        if (announcement == null) return NotFound();

        _announcementRepository.Delete(announcement);
        await _announcementRepository.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAllAnnouncements()
    {
        var announcements = await _announcementRepository.GetActiveAnnouncementsAsync();
        foreach (var ann in announcements)
        {
            _announcementRepository.Delete(ann);
        }
        await _announcementRepository.SaveChangesAsync();
        return NoContent();
    }
}

/// <summary>Request body for creating an announcement with optional specific recipient.</summary>
public class CreateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Type { get; set; }

    /// <summary>If set, the email notice is sent only to this employee instead of broadcasting.</summary>
    public int? TargetEmployeeId { get; set; }
}
