using Microsoft.AspNetCore.Mvc;
using MWMS.Application.Interfaces;

namespace MWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{
    private readonly IPositionService _positionService;

    public PositionsController(IPositionService positionService)
    {
        _positionService = positionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var positions = await _positionService.GetAllAsync();
        return Ok(positions);
    }

    [HttpPost("get-or-create")]
    public async Task<IActionResult> GetOrCreate([FromBody] PositionCreateRequest request)
    {
        var id = await _positionService.GetOrCreateAsync(request.Name);
        return Ok(new { id });
    }
}

public class PositionCreateRequest
{
    public string Name { get; set; } = string.Empty;
}
