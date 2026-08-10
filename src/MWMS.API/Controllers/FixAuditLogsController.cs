using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MWMS.Persistence.Context;
using System.Text.Json;

namespace MWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FixAuditLogsController : ControllerBase
{
    private readonly AppDbContext _context;

    public FixAuditLogsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("fix-negative-ids")]
    public async Task<IActionResult> FixNegativeIds()
    {
        var logs = await _context.AuditLogs.ToListAsync();
        var logsToFix = logs.Where(l => int.TryParse(l.EntityId, out int id) && id < 0).ToList();
        
        int fixedCount = 0;

        foreach (var log in logsToFix)
        {
            try
            {
                var newVals = JsonSerializer.Deserialize<Dictionary<string, object>>(log.NewValues);
                if (newVals == null) continue;

                int? realId = null;

                if (log.EntityName == "LeaveRequest")
                {
                    // Find matching LeaveRequest based on timestamp and EmployeeId
                    if (newVals.TryGetValue("EmployeeId", out var empIdObj) && int.TryParse(empIdObj.ToString(), out int empId))
                    {
                        var timeMin = log.Timestamp.AddMinutes(-5);
                        var timeMax = log.Timestamp.AddMinutes(5);
                        
                        var matchingRequest = await _context.LeaveRequests
                            .Where(lr => lr.EmployeeId == empId && lr.CreatedAt >= timeMin && lr.CreatedAt <= timeMax)
                            .OrderBy(lr => Math.Abs(EF.Functions.DateDiffSecond(lr.CreatedAt, log.Timestamp)))
                            .FirstOrDefaultAsync();
                            
                        if (matchingRequest != null)
                        {
                            realId = matchingRequest.Id;
                        }
                    }
                }
                else if (log.EntityName == "ApprovalHistory")
                {
                    if (newVals.TryGetValue("RequestId", out var reqIdObj) && int.TryParse(reqIdObj.ToString(), out int reqId))
                    {
                        var timeMin = log.Timestamp.AddMinutes(-5);
                        var timeMax = log.Timestamp.AddMinutes(5);
                        
                        var matchingApproval = await _context.Set<MWMS.Domain.Entities.ApprovalHistory>()
                            .Where(ah => ah.RequestId == reqId && ah.DecisionAt >= timeMin && ah.DecisionAt <= timeMax)
                            .OrderBy(ah => Math.Abs(EF.Functions.DateDiffSecond(ah.DecisionAt, log.Timestamp)))
                            .FirstOrDefaultAsync();
                            
                        if (matchingApproval != null)
                        {
                            realId = matchingApproval.Id;
                        }
                    }
                }

                if (realId.HasValue)
                {
                    log.EntityId = realId.Value.ToString();
                    
                    var pkName = "Id";
                    if (newVals.ContainsKey(pkName))
                    {
                        newVals[pkName] = realId.Value;
                        log.NewValues = JsonSerializer.Serialize(newVals);
                    }
                    
                    fixedCount++;
                }
            }
            catch
            {
                // Ignore parsing errors for individual logs
            }
        }

        if (fixedCount > 0)
        {
            await _context.SaveChangesAsync();
        }

        return Ok(new { Message = $"Fixed {fixedCount} logs with negative IDs." });
    }
}
