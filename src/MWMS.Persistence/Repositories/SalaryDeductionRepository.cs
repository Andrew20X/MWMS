using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Persistence.Context;

namespace MWMS.Persistence.Repositories;

public class SalaryDeductionRepository : GenericRepository<SalaryDeduction>, ISalaryDeductionRepository
{
    public SalaryDeductionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SalaryDeduction>> GetByEmployeeAsync(int employeeId)
    {
        return await _context.SalaryDeductions
            .Include(d => d.RelatedAttendance)
            .Where(d => d.EmployeeId == employeeId)
            .ToListAsync();
    }
}
