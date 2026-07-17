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
    public async Task<IActionResult> GetAll()
    {
        var employees = await _employeeService.GetAllAsync();

        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);

        if (employee == null)
            return NotFound();

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
        var deleted = await _employeeService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
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