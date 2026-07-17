using System.Collections.Generic;
using System.Threading.Tasks;
using MWMS.Domain.Entities;

namespace MWMS.Application.Interfaces;

public interface ICorrectionRepository : IGenericRepository<CorrectionRequest>
{
    Task<IEnumerable<CorrectionRequest>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<CorrectionRequest>> GetPendingRequestsAsync();
}
