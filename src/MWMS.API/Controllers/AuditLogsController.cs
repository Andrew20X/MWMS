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
            .ToListAsync();

        var adminIds = logs.Where(l => l.AdminUserId != null).Select(l => l.AdminUserId.Value).Distinct().ToList();
        var targetIds = logs.Where(l => l.TargetEmployeeId != null).Select(l => l.TargetEmployeeId.Value).Distinct().ToList();

        var users = await _context.Users.Where(u => adminIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Username);
        var employees = await _context.Employees.Where(e => targetIds.Contains(e.Id)).ToDictionaryAsync(e => e.Id, e => e.FirstName + " " + e.LastName);

        var result = logs.Select(l => new
        {
            l.Id,
            l.ActionType,
            l.EntityName,
            l.EntityId,
            l.OldValues,
            l.NewValues,
            l.Changes,
            l.Timestamp,
            AdminUser = l.AdminUserId != null && users.ContainsKey(l.AdminUserId.Value) ? users[l.AdminUserId.Value] : "System",
            TargetEmployee = l.TargetEmployeeId != null && employees.ContainsKey(l.TargetEmployeeId.Value) ? employees[l.TargetEmployeeId.Value] : null
        });

        return Ok(result);
    }
}
