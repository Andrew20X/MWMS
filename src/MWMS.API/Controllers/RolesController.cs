using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MWMS.Application.DTOs;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using System.Security.Claims;

namespace MWMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RolesController(IUserRepository userRepository, IEmployeeRepository employeeRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Changes the role of a user. (Admin only)
    /// </summary>
    [HttpPut("user/{userId}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangeRole(int userId, [FromBody] ChangeRoleDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.IsDeleted) return NotFound(new { error = "User not found." });

        var validRoles = new[] { "Employee", "Manager", "HR", "Admin" };
        if (!validRoles.Contains(dto.Role))
        {
            return BadRequest(new { error = "Invalid role." });
        }

        user.Role = dto.Role;
        user.UpdatedAt = DateTime.UtcNow;
        // _userRepository.Update(user); // EF Tracks this automatically
        await _userRepository.SaveChangesAsync();

        return Ok(new { message = "Role updated successfully." });
    }

    /// <summary>
    /// Alternatively, change the role using EmployeeId. (Admin only)
    /// </summary>
    [HttpPut("employee/{employeeId}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangeRoleByEmployeeId(int employeeId, [FromBody] ChangeRoleDto dto)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null) return NotFound(new { error = "Employee not found." });

        var username = $"EMP-SYNC-{employee.EmployeeCode}";
        var user = await _userRepository.GetByUsernameAsync(username);
        
        if (user == null && !string.IsNullOrEmpty(employee.Email))
        {
            var users = await _userRepository.GetAllAsync();
            user = users.FirstOrDefault(u => u.Email == employee.Email && !u.IsDeleted);
        }
        
        if (user == null)
        {
            var users = await _userRepository.GetAllAsync();
            user = users.FirstOrDefault(u => u.FullName == $"{employee.FirstName} {employee.LastName}" && !u.IsDeleted);
        }

        if (user == null)
        {
            user = new User
            {
                Username = username,
                PasswordHash = _passwordHasher.Hash(username),
                FullName = $"{employee.FirstName} {employee.LastName}",
                Email = employee.Email ?? "",
                Role = dto.Role,
                IsActive = true
            };
            await _userRepository.AddAsync(user);
        }
        else
        {
            user.Role = dto.Role;
            user.UpdatedAt = DateTime.UtcNow;
        }

        var validRoles = new[] { "Employee", "Manager", "HR", "Admin" };
        if (!validRoles.Contains(dto.Role))
        {
            return BadRequest(new { error = "Invalid role." });
        }

        await _userRepository.SaveChangesAsync();

        return Ok(new { message = "Role updated successfully.", role = dto.Role });
    }

    /// <summary>
    /// Gets a list of all users with the Manager role. (Admin only)
    /// </summary>
    [HttpGet("managers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetManagers()
    {
        var managerUsers = await _userRepository.GetByRoleAsync("Manager");
        var adminUsers = await _userRepository.GetByRoleAsync("Admin");
        var hrUsers = await _userRepository.GetByRoleAsync("HR");

        var allManagers = managerUsers.Concat(adminUsers).Concat(hrUsers).DistinctBy(u => u.Id);

        var result = new List<object>();
        foreach (var user in allManagers)
        {
            var employee = await _employeeRepository.GetByEmailAsync(user.Email);
            if (employee == null)
            {
                var employees = await _employeeRepository.GetAllAsync();
                employee = employees.FirstOrDefault(e => $"{e.FirstName} {e.LastName}" == user.FullName || $"EMP-SYNC-{e.EmployeeCode}" == user.Username);
            }

            if (employee != null)
            {
                result.Add(new
                {
                    EmployeeId = employee.Id,
                    FullName = user.FullName,
                    Role = user.Role,
                    EmployeeCode = employee.EmployeeCode
                });
            }
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets the organizational hierarchy starting from top-level managers. (Admin only)
    /// </summary>
    [HttpGet("hierarchy")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetHierarchy()
    {
        var employees = await _employeeRepository.GetAllAsync();
        var topLevel = employees.Where(e => !e.IsDeleted && e.ManagerId == null).ToList();

        object BuildTree(Employee emp)
        {
            var subordinates = employees.Where(e => !e.IsDeleted && e.ManagerId == emp.Id).ToList();
            return new
            {
                EmployeeId = emp.Id,
                FullName = $"{emp.FirstName} {emp.LastName}",
                Position = emp.Position?.Name,
                Subordinates = subordinates.Select(BuildTree).ToList()
            };
        }

        var tree = topLevel.Select(BuildTree).ToList();
        return Ok(tree);
    }

    /// <summary>
    /// Gets the employees managed by the current logged-in manager. (Manager only)
    /// </summary>
    [HttpGet("my-team")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> GetMyTeam()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (string.IsNullOrEmpty(employeeIdClaim)) return Unauthorized();

        var employeeId = int.Parse(employeeIdClaim);
        var team = await _employeeRepository.GetByManagerIdAsync(employeeId);
        
        var result = team.Select(e => new
        {
            EmployeeId = e.Id,
            FullName = $"{e.FirstName} {e.LastName}",
            Position = e.Position?.Name,
            EmployeeCode = e.EmployeeCode
        });

        return Ok(result);
    }
}
