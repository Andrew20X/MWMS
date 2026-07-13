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
}