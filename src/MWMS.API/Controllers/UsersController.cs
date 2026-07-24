using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MWMS.Application.DTOs;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Persistence.Context;

namespace MWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AppDbContext _context;

    public UsersController(
        IUserRepository userRepository, 
        IEmployeeRepository employeeRepository, 
        IPasswordHasher passwordHasher,
        AppDbContext context)
    {
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _passwordHasher = passwordHasher;
        _context = context;
    }

    [HttpPut("{employeeId}")]
    public async Task<IActionResult> UpdateUserAccount(int employeeId, [FromBody] UpdateUserAccountDto dto)
    {
        var adminUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int? adminUserId = null;
        if (int.TryParse(adminUserIdClaim, out int parsedId))
        {
            adminUserId = parsedId;
        }

        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null) return NotFound(new { error = "Employee not found." });

        var users = await _userRepository.GetAllAsync();
        
        var user = users.FirstOrDefault(u => 
            u.Username == $"EMP-SYNC-{employee.EmployeeCode}" || 
            (!string.IsNullOrEmpty(employee.Email) && u.Email == employee.Email) ||
            u.FullName == $"{employee.FirstName} {employee.LastName}");

        if (user == null) return NotFound(new { error = "Corresponding user account not found for this employee." });

        // Validate uniqueness
        if (users.Any(u => u.Id != user.Id && u.Username == dto.Username && !u.IsDeleted))
            return BadRequest(new { error = "Username is already taken." });
            
        if (!string.IsNullOrEmpty(dto.Email) && users.Any(u => u.Id != user.Id && u.Email == dto.Email && !u.IsDeleted))
            return BadRequest(new { error = "Email is already taken by another user." });

        var employees = await _employeeRepository.GetAllAsync();
        if (employees.Any(e => e.Id != employee.Id && e.EmployeeCode == dto.EmployeeCode && !e.IsDeleted))
            return BadRequest(new { error = "Employee Code is already in use." });

        // Capture old state for audit log
        var oldState = new {
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            EmployeeCode = employee.EmployeeCode,
            Role = user.Role,
            ManagerId = employee.ManagerId
        };

        // Update Position if needed
        var positions = await _context.Positions.ToListAsync();
        var position = positions.FirstOrDefault(p => p.Name == dto.PositionName);
        if (position == null && !string.IsNullOrEmpty(dto.PositionName))
        {
            position = new Position { Name = dto.PositionName };
            _context.Positions.Add(position);
            await _context.SaveChangesAsync();
        }

        // Apply changes
        user.Username = dto.Username;
        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.Role = dto.Role;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.PasswordHash = _passwordHasher.Hash(dto.Password);
        }

        employee.EmployeeCode = dto.EmployeeCode;
        employee.Email = dto.Email;
        employee.ManagerId = dto.ManagerId;
        
        if (dto.SubordinateIds != null)
        {
            var allEmployees = await _context.Employees.ToListAsync();
            var currentSubordinates = allEmployees.Where(e => e.ManagerId == employee.Id).ToList();
            
            foreach (var sub in currentSubordinates)
            {
                if (!dto.SubordinateIds.Contains(sub.Id))
                {
                    sub.ManagerId = null;
                }
            }
            
            foreach (var subId in dto.SubordinateIds)
            {
                var sub = allEmployees.FirstOrDefault(e => e.Id == subId);
                if (sub != null && sub.ManagerId != employee.Id)
                {
                    sub.ManagerId = employee.Id;
                }
            }
        }
        
        if (position != null)
        {
            employee.PositionId = position.Id;
        }
        
        var names = dto.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        employee.FirstName = names.Length > 0 ? names[0] : "";
        employee.LastName = names.Length > 1 ? string.Join(" ", names.Skip(1)) : employee.FirstName;
        employee.UpdatedAt = DateTime.UtcNow;

        // Log Changes
        var newState = new {
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            EmployeeCode = employee.EmployeeCode,
            Role = user.Role,
            ManagerId = employee.ManagerId
        };

        var auditLog = new AuditLog
        {
            AdminUserId = adminUserId,
            TargetEmployeeId = employee.Id,
            Changes = JsonSerializer.Serialize(new { Old = oldState, New = newState }),
            Timestamp = DateTime.UtcNow
        };
        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync();

        return Ok(new { message = "User account updated successfully." });
    }
}
