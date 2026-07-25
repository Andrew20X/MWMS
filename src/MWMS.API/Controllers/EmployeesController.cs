using Microsoft.AspNetCore.Mvc;
using MWMS.Application.DTOs;
using MWMS.Application.Services;
using MWMS.Domain.Entities;

namespace MWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromServices] MWMS.Persistence.Context.AppDbContext context)
    {
        var employees = await _employeeService.GetAllAsync();
        var users = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(context.Users);

        foreach(var emp in employees)
        {
            var user = users.FirstOrDefault(u => 
                !u.IsDeleted && !u.Username.StartsWith("EMP-SYNC") && !u.Username.StartsWith("MANAGER-SYNC") &&
                ((!string.IsNullOrEmpty(emp.Email) && emp.Email != "(No Email)" && u.Email == emp.Email) ||
                 u.FullName == $"{emp.FirstName} {emp.LastName}"))
                ?? users.FirstOrDefault(u => 
                    !u.IsDeleted && (u.Username == $"EMP-SYNC-{emp.EmployeeCode}" || u.Username == $"MANAGER-SYNC-{emp.EmployeeCode}"));
            emp.Role = user?.Role ?? "Employee";
            emp.Username = user?.Username ?? (emp.Role == "Manager" ? $"MANAGER-SYNC-{emp.EmployeeCode}" : $"EMP-SYNC-{emp.EmployeeCode}");
            if (emp.Manager != null)
            {
                emp.ManagerName = emp.Manager.FirstName + " " + emp.Manager.LastName;
            }
            if (emp.Subordinates != null && emp.Subordinates.Any())
            {
                emp.SubordinateIds = emp.Subordinates.Select(s => s.Id).ToList();
                emp.SubordinatesList = string.Join(", ", emp.Subordinates.Select(s => s.FirstName + " " + s.LastName));
                
                // Dynamically bump role to Manager if they have subordinates
                if (emp.Role == "Employee")
                {
                    emp.Role = "Manager";
                    // If they are a generated account, fix the username display
                    if (emp.Username.StartsWith("EMP-SYNC-"))
                    {
                        emp.Username = $"MANAGER-SYNC-{emp.EmployeeCode}";
                    }
                }
            }
        }

        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);

        if (employee == null)
            return NotFound();

        var context = HttpContext.RequestServices.GetRequiredService<MWMS.Persistence.Context.AppDbContext>();
        var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(context.Users, u => 
            !u.IsDeleted && !u.Username.StartsWith("EMP-SYNC") && !u.Username.StartsWith("MANAGER-SYNC") &&
            ((!string.IsNullOrEmpty(employee.Email) && employee.Email != "(No Email)" && u.Email == employee.Email) ||
             u.FullName == employee.FirstName + " " + employee.LastName))
            ?? await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(context.Users, u => 
                !u.IsDeleted && (u.Username == "EMP-SYNC-" + employee.EmployeeCode || u.Username == "MANAGER-SYNC-" + employee.EmployeeCode));
        
        employee.Role = user?.Role ?? "Employee";
        employee.Username = user?.Username ?? (employee.Role == "Manager" ? $"MANAGER-SYNC-{employee.EmployeeCode}" : $"EMP-SYNC-{employee.EmployeeCode}");

        return Ok(employee);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeDto dto)
    {
        var employee = new Employee
        {
            EmployeeCode = dto.EmployeeCode,
            DeviceUserId = dto.DeviceUserId,
            FirstName = dto.FirstName,
            MiddleName = dto.MiddleName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            DepartmentId = dto.DepartmentId,
            PositionId = dto.PositionId,
            ShiftId = dto.ShiftId,
            ManagerId = dto.ManagerId,
            SubordinateIds = dto.SubordinateIds,
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true
        };

        try
        {
            var created = await _employeeService.CreateAsync(employee);

            return CreatedAtAction(nameof(GetById),
                new { id = created.Id },
                created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateEmployeeDto dto)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null) return NotFound();

        employee.EmployeeCode = dto.EmployeeCode;
        employee.DeviceUserId = dto.DeviceUserId;
        employee.FirstName = dto.FirstName;
        employee.MiddleName = dto.MiddleName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.Phone = dto.Phone;
        employee.DepartmentId = dto.DepartmentId;
        employee.PositionId = dto.PositionId;
        employee.ShiftId = dto.ShiftId;
        employee.ManagerId = dto.ManagerId;
        employee.SubordinateIds = dto.SubordinateIds;

        try
        {
            var updated = await _employeeService.UpdateAsync(id, employee);
            if (updated == null) return NotFound();
            
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var context = HttpContext.RequestServices.GetRequiredService<MWMS.Persistence.Context.AppDbContext>();
        var employee = await context.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // 1. Delete associated user account(s)
            var users = context.Users.Where(u => 
                u.Username == $"MANAGER-SYNC-{employee.EmployeeCode}" ||
                u.Username == $"EMP-SYNC-{employee.EmployeeCode}" ||
                (!string.IsNullOrEmpty(employee.Email) && u.Email == employee.Email) ||
                u.FullName == $"{employee.FirstName} {employee.LastName}");
            context.Users.RemoveRange(users);

            // 2. Delete all related records to avoid FK constraint errors
            context.SalaryDeductions.RemoveRange(context.SalaryDeductions.Where(x => x.EmployeeId == id));
            context.ApprovalHistories.RemoveRange(context.ApprovalHistories.Where(x => x.ApproverId == id));
            context.LeaveBalances.RemoveRange(context.LeaveBalances.Where(x => x.EmployeeId == id));
            context.LeaveRequests.RemoveRange(context.LeaveRequests.Where(x => x.EmployeeId == id));
            context.OvertimeRequests.RemoveRange(context.OvertimeRequests.Where(x => x.EmployeeId == id));
            context.CorrectionRequests.RemoveRange(context.CorrectionRequests.Where(x => x.EmployeeId == id));
            context.RawAttendanceLogs.RemoveRange(context.RawAttendanceLogs.Where(x => x.EmployeeId == id));
            context.Attendances.RemoveRange(context.Attendances.Where(x => x.EmployeeId == id));

            // Set subordinates manager to null
            var subordinates = context.Employees.Where(e => e.ManagerId == id);
            foreach(var sub in subordinates) { sub.ManagerId = null; }

            // 3. Finally delete the employee
            context.Employees.Remove(employee);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(new { error = "Failed to completely delete the employee and their data.", details = ex.Message });
        }
    }

    [HttpPost("import-fb")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> ImportFbEmployees([FromServices] MWMS.Persistence.Context.AppDbContext context)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "DELETE FROM OvertimeRequests");
            await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "DELETE FROM CorrectionRequests");
            await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "DELETE FROM LeaveRequests");
            await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "DELETE FROM Attendances");
            await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "DELETE FROM RawAttendanceLogs");
            try { await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "UPDATE Users SET EmployeeId = NULL"); } catch { }
            await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "DELETE FROM Employees");

            var department = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(context.Departments);
            var position = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(context.Positions);
            var shift = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(context.Shifts);

            int deptId = department?.Id ?? 1;
            int posId = position?.Id ?? 1;
            int shiftId = shift?.Id ?? 1;

            using var workbook = new ClosedXML.Excel.XLWorkbook(@"D:\MWMS\FB_temp.xlsx");
            var worksheet = workbook.Worksheet(1);
            var range = worksheet.RangeUsed();
            var rows = range != null ? range.RowsUsed().Skip(1) : Enumerable.Empty<ClosedXML.Excel.IXLRangeRow>();
            int counter = 1;
            foreach (var row in rows)
            {
                var idStr = row.Cell(1).GetString().Trim();
                var nameStr = row.Cell(2).GetString().Trim();
                var emailStr = row.Cell(3).GetString().Trim();

                if (string.IsNullOrWhiteSpace(nameStr) || nameStr == "_") continue;

                var empCode = "";
                int deviceUserId = 0;

                if (string.IsNullOrWhiteSpace(idStr) || idStr == "_")
                {
                    empCode = $"no ID - {counter}";
                }
                else
                {
                    empCode = idStr;
                    if (int.TryParse(idStr, out int parsedId))
                    {
                        deviceUserId = parsedId;
                    }
                }

                string email = "(No Email)";
                if (!string.IsNullOrWhiteSpace(emailStr) && emailStr != "_")
                {
                    email = emailStr;
                }

                var names = nameStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string firstName = names.Length > 0 ? names[0] : "Unknown";
                string lastName = names.Length > 1 ? string.Join(" ", names.Skip(1)) : firstName;

                if (firstName.Length > 50) firstName = firstName.Substring(0, 50);
                if (lastName.Length > 50) lastName = lastName.Substring(0, 50);

                var employee = new Employee
                {
                    EmployeeCode = empCode,
                    DeviceUserId = deviceUserId,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    DepartmentId = deptId,
                    PositionId = posId,
                    ShiftId = shiftId,
                    HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    IsActive = true
                };

                context.Employees.Add(employee);
                counter++;
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = $"Imported {counter - 1} employees." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(new { error = ex.Message, inner = ex.InnerException?.Message, stack = ex.StackTrace });
        }
    }
}