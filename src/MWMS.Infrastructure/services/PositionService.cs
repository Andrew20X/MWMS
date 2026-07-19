using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;

namespace MWMS.Infrastructure.Services;

public class PositionService : IPositionService
{
    private readonly IPositionRepository _positionRepository;

    public PositionService(IPositionRepository positionRepository)
    {
        _positionRepository = positionRepository;
    }

    public async Task<IEnumerable<Position>> GetAllAsync()
    {
        return await _positionRepository.GetAllAsync();
    }

    public async Task<int> GetOrCreateAsync(string positionName)
    {
        if (string.IsNullOrWhiteSpace(positionName)) return 1;
        var all = await _positionRepository.GetAllAsync();
        var existing = all.FirstOrDefault(p => p.Name.Equals(positionName, StringComparison.OrdinalIgnoreCase));
        if (existing != null) return existing.Id;

        var newPos = new Position { Name = positionName };
        await _positionRepository.AddAsync(newPos);
        await _positionRepository.SaveChangesAsync();
        return newPos.Id;
    }
}
