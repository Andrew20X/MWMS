using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWMS.Application.DTOs.Attendance;
using MWMS.Application.Interfaces;

namespace MWMS.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,HR,Manager")]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public ReportsController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] AttendanceFilterDto filter)
    {
        var result = await _attendanceService.SearchAttendanceAsync(filter);
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] AttendanceFilterDto filter, [FromQuery] string format = "excel")
    {
        try
        {
            var fileBytes = await _attendanceService.ExportReportsAsync(filter, format);
            
            var fileName = $"AttendanceReport_{DateTime.Now:yyyyMMddHHmmss}";
            string contentType;

            if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".csv";
                contentType = "text/csv";
            }
            else if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".pdf";
                contentType = "application/pdf";
            }
            else
            {
                fileName += ".xlsx";
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred during export: {ex.Message}");
        }
    }
}
