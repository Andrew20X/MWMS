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
            emp = allEmps.FirstOrDefault(e => e.Id == 235);
            if (emp != null) {
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 236);
            if (emp != null) {
                emp.PositionId = await GetPos("Administrative Assistant Manager");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 237);
            if (emp != null) {
                emp.PositionId = await GetPos("Project Manager");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 238);
            if (emp != null) {
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 239);
            if (emp != null) {
                emp.PositionId = await GetPos("Buffet");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 243);
            if (emp != null) {
                emp.PositionId = await GetPos("Purchasing Officer");
                emp.FirstName = "Ibrahim";
                emp.LastName = "Mahmoud Abdel Latif Ibrahim Abdel Sayed El-Minshawy";
                if (emp.FirstName.Length > 50) emp.FirstName = emp.FirstName.Substring(0, 50);
                if (emp.LastName.Length > 50) emp.LastName = emp.LastName.Substring(0, 50);
                var user = allUsers.FirstOrDefault(u => u.Email == emp.Email || u.FullName == emp.FirstName + " " + emp.LastName);
                if (user != null) user.FullName = "Ibrahim Mahmoud Abdel Latif Ibrahim Abdel Sayed El-Minshawy";
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 244);
            if (emp != null) {
                emp.PositionId = await GetPos("Maintenance Engineer");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 245);
            if (emp != null) {
                emp.PositionId = await GetPos("Buffet");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 246);
            if (emp != null) {
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 248);
            if (emp != null) {
                emp.PositionId = await GetPos("Occupational Health and Safety Engineer");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 249);
            if (emp != null) {
                emp.PositionId = await GetPos("Control Engineer");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 250);
            if (emp != null) {
                emp.PositionId = await GetPos("Metal Forming Technician");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 251);
            if (emp != null) {
                emp.PositionId = await GetPos("Mechanical Engineer Technical Office");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 252);
            if (emp != null) {
                emp.PositionId = await GetPos("Purchasing Supervisor");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 253);
            if (emp != null) {
                emp.PositionId = await GetPos("Operations Manager");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 254);
            if (emp != null) {
                emp.PositionId = await GetPos("Technical Sales Engineer");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 255);
            if (emp != null) {
                emp.PositionId = await GetPos("Purchasing Representative");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 256);
            if (emp != null) {
                emp.PositionId = await GetPos("Purchasing Representative");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 257);
            if (emp != null) {
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 258);
            if (emp != null) {
                emp.PositionId = await GetPos("Procurement assistant");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 259);
            if (emp != null) {
                emp.PositionId = await GetPos("Security personnel");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 260);
            if (emp != null) {
                emp.PositionId = await GetPos("Web Developer");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 261);
            if (emp != null) {
                emp.PositionId = await GetPos("Warehouse Manager assistant");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 262);
            if (emp != null) {
                emp.PositionId = await GetPos("Finance Manager");
                emp.Email = "amr.mabrouk@measuresofteg.com";
                var user = allUsers.FirstOrDefault(u => u.Email == emp.Email || u.FullName == emp.FirstName + " " + emp.LastName);
                if (user != null) user.Email = "amr.mabrouk@measuresofteg.com";
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 265);
            if (emp != null) {
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 266);
            if (emp != null) {
                emp.PositionId = await GetPos("HR Specialist");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 267);
            if (emp != null) {
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 268);
            if (emp != null) {
                emp.PositionId = await GetPos("Document Controler");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 269);
            if (emp != null) {
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 270);
            if (emp != null) {
                emp.PositionId = await GetPos("Treasury officer");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 271);
            if (emp != null) {
                emp.PositionId = await GetPos("Senior Supplier Accountant");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 272);
            if (emp != null) {
                emp.PositionId = await GetPos("Driver");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 273);
            if (emp != null) {
                emp.PositionId = await GetPos("I & C Engineer");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 274);
            if (emp != null) {
                emp.PositionId = await GetPos("Sales Representative");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 275);
            if (emp != null) {
                emp.PositionId = await GetPos("HR & Administration Coordinator");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 276);
            if (emp != null) {
                emp.PositionId = await GetPos("Legal Officer");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 278);
            if (emp != null) {
                emp.PositionId = await GetPos("Security personnel");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 279);
            if (emp != null) {
                emp.PositionId = await GetPos("Office Manager");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 280);
            if (emp != null) {
                emp.PositionId = await GetPos("AI Developer");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 281);
            if (emp != null) {
                emp.PositionId = await GetPos("Web Developer");
                emp.FirstName = "Ziad";
                emp.LastName = "Adel Hassan Ibrahim Dorra";
                var user = allUsers.FirstOrDefault(u => u.Email == emp.Email || u.FullName == emp.FirstName + " " + emp.LastName);
                if (user != null) user.FullName = "Ziad Adel Hassan Ibrahim Dorra";
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 282);
            if (emp != null) {
                emp.PositionId = await GetPos("I & C Engineer");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 283);
            if (emp != null) {
                emp.PositionId = await GetPos("Accounts Receivable Accountant");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 284);
            if (emp != null) {
                emp.PositionId = await GetPos("I & C Engineer");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 285);
            if (emp != null) {
                emp.PositionId = await GetPos("Draftsman");
                updatedCount++;
            }
            emp = allEmps.FirstOrDefault(e => e.Id == 286);
            if (emp != null) {
                emp.PositionId = await GetPos("I & C Engineer");
                updatedCount++;
            }
            // Fix Roles
            var usersWithUserRole = allUsers.Where(u => u.Role == "User").ToList();
            foreach (var u in usersWithUserRole)
            {
                u.Role = "Employee";
                updatedCount++;
            }

            // Remove @company.com emails
            var empsWithCompanyEmail = allEmps.Where(e => e.Email != null && e.Email.EndsWith("@company.com", StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var e in empsWithCompanyEmail)
            {
                e.Email = "(No Email)";
                updatedCount++;
            }

            var usersWithCompanyEmail = allUsers.Where(u => u.Email != null && u.Email.EndsWith("@company.com", StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var u in usersWithCompanyEmail)
            {
                u.Email = "";
                updatedCount++;
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = $"Successfully applied updates for {updatedCount} employees/users." });
        } catch (Exception ex) {
            return StatusCode(500, new { Error = ex.Message, Inner = ex.InnerException?.Message, Stack = ex.StackTrace });
        }
    }
}
