using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Persistence.Context;

namespace MWMS.Persistence.Repositories;

public class OvertimeRepository : GenericRepository<OvertimeRequest>, IOvertimeRepository
{
    public OvertimeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<OvertimeRequest>> GetByEmployeeAsync(int employeeId)
    {
        return await _dbSet
            .Include(o => o.Employee)
            .Where(o => o.EmployeeId == employeeId && !o.IsDeleted)
            .OrderByDescending(o => o.Date)
            .ToListAsync();
    }

    public override async Task<IEnumerable<OvertimeRequest>> GetAllAsync()
    {
        return await _dbSet
            .Include(o => o.Employee)
            .Where(o => !o.IsDeleted)
            .OrderByDescending(o => o.Date)
            .ToListAsync();
    }
}
