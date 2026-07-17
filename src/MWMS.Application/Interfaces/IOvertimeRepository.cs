using System.Collections.Generic;
using System.Threading.Tasks;
using MWMS.Domain.Entities;

namespace MWMS.Application.Interfaces;

public interface IOvertimeRepository : IGenericRepository<OvertimeRequest>
{
    Task<IEnumerable<OvertimeRequest>> GetByEmployeeAsync(int employeeId);
}
