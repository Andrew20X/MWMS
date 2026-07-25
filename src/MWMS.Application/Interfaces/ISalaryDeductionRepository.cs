using System.Collections.Generic;
using System.Threading.Tasks;
using MWMS.Domain.Entities;

namespace MWMS.Application.Interfaces;

public interface ISalaryDeductionRepository : IGenericRepository<SalaryDeduction>
{
    Task<IEnumerable<SalaryDeduction>> GetByEmployeeAsync(int employeeId);
}
