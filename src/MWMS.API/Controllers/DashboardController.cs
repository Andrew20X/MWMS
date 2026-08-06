using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWMS.Application.Interfaces;

namespace MWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetTodayStats()
    {
        var stats = await _dashboardService.GetTodayStatsAsync();
        return Ok(stats);
    }

    [HttpGet("trend")]
    public async Task<IActionResult> GetAttendanceTrend([FromQuery] int days = 7)
    {
        var trend = await _dashboardService.GetAttendanceTrendAsync(days);
        return Ok(trend);
    }

    [HttpGet("live")]
    public async Task<IActionResult> GetLiveAttendance()
    {
        var liveList = await _dashboardService.GetLiveAttendanceAsync();
        return Ok(liveList);
    }

    [HttpGet("late")]
    public async Task<IActionResult> GetLateArrivals()
    {
        var list = await _dashboardService.GetLateArrivalsTodayAsync();
        return Ok(list);
    }

    [HttpGet("absent")]
    public async Task<IActionResult> GetAbsents()
    {
        var list = await _dashboardService.GetAbsentsTodayAsync();
        return Ok(list);
    }
    [HttpGet("present")]
    public async Task<IActionResult> GetPresent()
    {
        var list = await _dashboardService.GetPresentTodayAsync();
        return Ok(list);
    }

    [HttpGet("workforce")]
    public async Task<IActionResult> GetWorkforce()
    {
        var list = await _dashboardService.GetWorkforceAsync();
        return Ok(list);
    }
}
