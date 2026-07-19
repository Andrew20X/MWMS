using MWMS.Domain.Entities;

namespace MWMS.Application.Interfaces;

public interface IPositionService
{
    Task<IEnumerable<Position>> GetAllAsync();
    Task<int> GetOrCreateAsync(string positionName);
}
