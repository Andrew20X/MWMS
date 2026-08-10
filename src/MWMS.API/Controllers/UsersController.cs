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

    [HttpGet("cleanup-duplicates")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> CleanupDuplicates()
    {
        var users = await _userRepository.GetAllAsync();
        var duplicatesToRemove = new List<User>();

        // Find users with the same email
        var groupedByEmail = users.Where(u => !string.IsNullOrEmpty(u.Email) && !u.IsDeleted).GroupBy(u => u.Email);
        foreach (var group in groupedByEmail)
        {
            if (group.Count() > 1)
            {
                // We have duplicates. Keep the manual one (not starting with EMP-SYNC or MANAGER-SYNC), delete the others.
                var manualUsers = group.Where(u => !u.Username.StartsWith("EMP-SYNC") && !u.Username.StartsWith("MANAGER-SYNC")).ToList();
                if (manualUsers.Any())
                {
                    var autoUsers = group.Where(u => u.Username.StartsWith("EMP-SYNC") || u.Username.StartsWith("MANAGER-SYNC")).ToList();
                    duplicatesToRemove.AddRange(autoUsers);
                }
            }
        }

        foreach (var dup in duplicatesToRemove)
        {
            _context.Users.Remove(dup);
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = $"Cleaned up {duplicatesToRemove.Count} duplicate auto-generated users." });
    }

    [HttpGet("by-employee/{employeeId}")]
    public async Task<IActionResult> GetUserByEmployeeId(int employeeId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null) return NotFound(new { error = "Employee not found." });

        var users = await _userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u => 
            !u.IsDeleted && !u.Username.StartsWith("EMP-SYNC") && !u.Username.StartsWith("MANAGER-SYNC") &&
            ((!string.IsNullOrEmpty(employee.Email) && employee.Email != "(No Email)" && u.Email == employee.Email) ||
             u.FullName == $"{employee.FirstName} {employee.LastName}"))
            ?? users.FirstOrDefault(u => 
                !u.IsDeleted && (u.Username == $"EMP-SYNC-{employee.EmployeeCode}" || u.Username == $"MANAGER-SYNC-{employee.EmployeeCode}"));

        if (user == null) return NotFound(new { error = "Corresponding user account not found for this employee." });

        return Ok(new
        {
            user.Id,
            user.Username,
            user.FullName,
            user.Email,
            user.Role
        });
    }

    [HttpPut("{employeeId:int}")]
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
            !u.IsDeleted && !u.Username.StartsWith("EMP-SYNC") && !u.Username.StartsWith("MANAGER-SYNC") &&
            ((!string.IsNullOrEmpty(employee.Email) && employee.Email != "(No Email)" && u.Email == employee.Email) ||
             u.FullName == $"{employee.FirstName} {employee.LastName}"))
            ?? users.FirstOrDefault(u => 
                !u.IsDeleted && (u.Username == $"EMP-SYNC-{employee.EmployeeCode}" || u.Username == $"MANAGER-SYNC-{employee.EmployeeCode}"));


        if (dto.EmployeeCode == "no ID") dto.EmployeeCode = "";
        
        // Validate uniqueness first
        var conflictingUsernameUser = users.FirstOrDefault(u => (user == null || u.Id != user.Id) && u.Username == dto.Username && !u.IsDeleted);
        if (conflictingUsernameUser != null)
        {
            if (conflictingUsernameUser.Username.StartsWith("EMP-SYNC") || conflictingUsernameUser.Username.StartsWith("MANAGER-SYNC"))
            {
                _context.Users.Remove(conflictingUsernameUser);
            }
            else
            {
                return BadRequest(new { error = "Username is already taken." });
            }
        }
            
        if (user == null || dto.Email != user.Email)
        {
            var conflictingEmailUser = users.FirstOrDefault(u => (user == null || u.Id != user.Id) && u.Email == dto.Email && !u.IsDeleted);
            if (conflictingEmailUser != null && !string.IsNullOrEmpty(dto.Email) && dto.Email != "(No Email)")
            {
                if (conflictingEmailUser.Username.StartsWith("EMP-SYNC") || conflictingEmailUser.Username.StartsWith("MANAGER-SYNC"))
                {
                    _context.Users.Remove(conflictingEmailUser);
                }
                else
                {
                    return BadRequest(new { error = $"Email is already taken by another user account: '{conflictingEmailUser.Username}'. Please use a different email or delete the duplicate account." });
                }
            }
        }

        bool isNewUser = false;
        if (user == null)
        {
            isNewUser = true;
            user = new User
            {
                Username = dto.Username,
                PasswordHash = _passwordHasher.Hash(string.IsNullOrEmpty(dto.Password) ? "measuresoft" : dto.Password),
                FullName = dto.FullName,
                Email = dto.Email ?? "",
                Role = dto.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
        }

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
        user.Email = dto.Email ?? "";
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
