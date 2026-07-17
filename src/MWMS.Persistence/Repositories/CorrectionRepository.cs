using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Persistence.Context;

namespace MWMS.Persistence.Repositories;

public class CorrectionRepository : GenericRepository<CorrectionRequest>, ICorrectionRepository
{
    public CorrectionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CorrectionRequest>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _dbSet
            .Include(c => c.Employee)
            .Where(c => c.EmployeeId == employeeId && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<CorrectionRequest>> GetPendingRequestsAsync()
    {
        return await _dbSet
            .Include(c => c.Employee)
            .Where(c => c.Status == "Pending" && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }
}
