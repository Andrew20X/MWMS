using Microsoft.EntityFrameworkCore;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Persistence.Context;

namespace MWMS.Persistence.Repositories;

public class LeaveBalanceRepository : GenericRepository<LeaveBalance>, ILeaveBalanceRepository
{
    public LeaveBalanceRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<LeaveBalance?> GetByEmployeeAndYearAsync(int employeeId, int year)
    {
        return await _dbSet
            .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId && lb.Year == year && !lb.IsDeleted);
    }

    public async Task<LeaveBalance> GetOrCreateAsync(int employeeId, int year)
    {
        var balance = await GetByEmployeeAndYearAsync(employeeId, year);
        if (balance != null) return balance;

        // Auto-create with defaults
        balance = new LeaveBalance
        {
            EmployeeId = employeeId,
            Year = year,
            AnnualLeaveTotal = 15,
            AnnualLeaveUsed = 0,
            EmergencyLeaveTotal = 6,
            EmergencyLeaveUsed = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _dbSet.AddAsync(balance);
        await _context.SaveChangesAsync();

        return balance;
    }
}
