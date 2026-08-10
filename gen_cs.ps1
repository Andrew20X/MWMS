$lines = Get-Content "d:\MWMS\plan_draft.md"
$cs = @"
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MWMS.Domain.Entities;
using MWMS.Persistence.Context;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FixNamesController : ControllerBase
{
    private readonly AppDbContext _context;

    public FixNamesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("fix-positions")]
    public async Task<IActionResult> FixPositionsAndCodes()
    {
        try {
            var allEmps = await _context.Employees.ToListAsync();
            var allUsers = await _context.Users.ToListAsync();
            var positions = await _context.Positions.ToListAsync();
            int updatedCount = 0;

            async Task<int> GetPos(string title) {
                var p = positions.FirstOrDefault(x => x.Name == title);
                if (p == null) {
                    p = new Position { Name = title, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                    _context.Positions.Add(p);
                    await _context.SaveChangesAsync();
                    positions.Add(p);
                }
                return p.Id;
            }

            Employee emp = null;

"@

$currentEmpId = -1

foreach ($line in $lines) {
    if ($line -match '^\- \*\*Matched DB Employee\*\*: .*? \(ID: (\d+), DeviceUserId: (\d+)\)') {
        $currentEmpId = $matches[1]
        $cs += "            emp = allEmps.FirstOrDefault(e => e.Id == $currentEmpId);`n"
        $cs += "            if (emp != null) {`n"
    }
    elseif ($line -match '^\- \[x\] Update Job Title: .*? \-\> (.*)$') {
        $newTitle = $matches[1]
        $cs += "                emp.PositionId = await GetPos(`"$newTitle`");`n"
    }
    elseif ($line -match '^\- \[x\] Update Fingerprint ID: \d+ \-\> (\d+)$') {
        $newFp = $matches[1]
        $cs += "                emp.DeviceUserId = $newFp;`n"
    }
    elseif ($line -match '^\- \[x\] Update Email: .*? \-\> (.*)$') {
        $newEmail = $matches[1]
        $cs += "                emp.Email = `"$newEmail`";`n"
        $cs += "                var user = allUsers.FirstOrDefault(u => u.Email == emp.Email || u.FullName == emp.FirstName + `" `" + emp.LastName);`n"
        $cs += "                if (user != null) user.Email = `"$newEmail`";`n"
    }
    elseif ($line -match '^\- \[x\] Update Name: .*? \-\> (.*)$') {
        $newName = $matches[1]
        $parts = $newName -split ' '
        $fn = $parts[0]
        $ln = ""
        if ($parts.Length -gt 1) {
            $ln = $parts[1..($parts.Length-1)] -join ' '
        }
        $cs += "                emp.FirstName = `"$fn`";`n"
        $cs += "                emp.LastName = `"$ln`";`n"
        $cs += "                var user = allUsers.FirstOrDefault(u => u.Email == emp.Email || u.FullName == emp.FirstName + `" `" + emp.LastName);`n"
        $cs += "                if (user != null) user.FullName = `"$newName`";`n"
    }
    elseif ($currentEmpId -ne -1 -and ($line -match '^\#\#\#' -or $line -eq "")) {
        $cs += "                updatedCount++;`n"
        $cs += "            }`n"
        $currentEmpId = -1
    }
}
if ($currentEmpId -ne -1) {
    $cs += "                updatedCount++;`n"
    $cs += "            }`n"
}

$cs += @"
            await _context.SaveChangesAsync();
            return Ok(new { Message = $"Successfully applied updates for {updatedCount} employees." });
        } catch (Exception ex) {
            return StatusCode(500, new { Error = ex.Message, Inner = ex.InnerException?.Message, Stack = ex.StackTrace });
        }
    }
}
"@

$cs | Out-File "d:\MWMS\src\MWMS.API\Controllers\FixNamesController.cs" -Encoding utf8
