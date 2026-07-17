using Microsoft.AspNetCore.Mvc;
using MWMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace MWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestEmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public TestEmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SendTestEmail([FromQuery] string to)
    {
        if (string.IsNullOrEmpty(to))
            return BadRequest(new { message = "Please provide a 'to' email address in the query parameters." });

        try
        {
            string subject = "Test Email from MWMS System";
            string htmlBody = @"
                <h2>MWMS Email Service Test</h2>
                <p>This is a test email sent from the MWMS System using the newly configured SMTP settings.</p>
                <p>If you received this, the email service is working correctly!</p>";
                
            await _emailService.SendEmailAsync(to, subject, htmlBody);
            
            return Ok(new { message = $"Test email successfully sent to {to}." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to send test email.", error = ex.Message });
        }
    }

    [HttpGet("check-user")]
    public async Task<IActionResult> CheckUser([FromServices] MWMS.Application.Interfaces.IUserRepository repo, [FromServices] MWMS.Application.Interfaces.IEmployeeRepository empRepo, string username, string code)
    {
        var user = await repo.GetByUsernameAsync(username);
        var emp = await empRepo.GetByEmployeeCodeAsync(code);
        return Ok(new { 
            UserEmail = user?.Email, 
            EmployeeEmail = emp?.Email 
        });
    }

    [HttpGet("parse-excel")]
    public IActionResult ParseExcel([FromQuery] string file = "Timesheet_xls.xlsx")
    {
        var path = $@"D:\MWMS\{file}";
        if (!System.IO.File.Exists(path)) return NotFound("File not found");

        var lines = new System.Collections.Generic.List<object>();
        try {
            using var workbook = new ClosedXML.Excel.XLWorkbook(path);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed()?.RowsUsed().Take(10);
            if (rows != null) {
                foreach (var row in rows) {
                    lines.Add(new {
                        Row = row.RowNumber(),
                        ColA = row.Cell(1).Value.ToString(),
                        ColB = row.Cell(2).Value.ToString(),
                        ColC = row.Cell(3).Value.ToString()
                    });
                }
            }
        } catch (Exception ex) {
            return BadRequest(ex.Message);
        }
        return Ok(lines);
    }
}
