using Microsoft.AspNetCore.Mvc;
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
}
