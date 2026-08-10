using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MWMS.Persistence.Context;
using System.Linq;
using System.Threading.Tasks;

namespace MWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuditLogsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int limit = 100)
    {
        var logs = await _context.AuditLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .Select(l => new
            {
                l.Id,
                l.ActionType,
                l.EntityName,
                l.EntityId,
                l.OldValues,
                l.NewValues,
                l.Changes,
                l.Timestamp,
                AdminUser = l.AdminUserId != null ? _context.Users.Where(u => u.Id == l.AdminUserId).Select(u => u.Username).FirstOrDefault() : "System",
                TargetEmployee = l.TargetEmployeeId != null ? _context.Employees.Where(e => e.Id == l.TargetEmployeeId).Select(e => e.FirstName + " " + e.LastName).FirstOrDefault() : null
            })
            .ToListAsync();

        return Ok(logs);
    }
}
