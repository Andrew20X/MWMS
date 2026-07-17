using Microsoft.EntityFrameworkCore;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Persistence.Context;

namespace MWMS.Persistence.Repositories;

public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<Employee?> GetByEmployeeCodeAsync(string employeeCode)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.Shift)
            .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);
    }

    public async Task<Employee?> GetByDeviceUserIdAsync(int deviceUserId)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.Shift)
            .FirstOrDefaultAsync(e => e.DeviceUserId == deviceUserId);
    }

    public async Task<Employee?> GetByEmailAsync(string email)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.Shift)
            .FirstOrDefaultAsync(e => e.Email == email);
    }

    public override async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.Shift)
            .ToListAsync();
    }

    public override async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.Shift)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<string?> ValidateReferencesAsync(int departmentId, int positionId, int shiftId)
    {
        if (departmentId <= 0)
            return "DepartmentId must be a valid department id (e.g. 1).";

        if (positionId <= 0)
            return "PositionId must be a valid position id (e.g. 1).";

        if (shiftId <= 0)
            return "ShiftId must be a valid shift id (e.g. 1).";

        if (!await _context.Departments.AnyAsync(d => d.Id == departmentId))
            return $"Department with Id {departmentId} does not exist.";

        if (!await _context.Positions.AnyAsync(p => p.Id == positionId))
            return $"Position with Id {positionId} does not exist.";

        if (!await _context.Shifts.AnyAsync(s => s.Id == shiftId))
            return $"Shift with Id {shiftId} does not exist.";

        return null;
    }
}