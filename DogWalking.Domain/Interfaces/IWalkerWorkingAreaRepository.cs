using DogWalking.Domain.Entities;

namespace DogWalking.Domain.Interfaces;

/// <summary>
/// Persistence contract for the zones where a walker operates.
/// </summary>
public interface IWalkerWorkingAreaRepository
{
    Task<WalkerWorkingArea?>             GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<WalkerWorkingArea>> GetByWalkerIdAsync(int walkerId, CancellationToken ct = default);
    Task<IEnumerable<WalkerWorkingArea>> GetAllAsync(CancellationToken ct = default);
    Task                                 AddAsync(WalkerWorkingArea area, CancellationToken ct = default);
    void                                 Remove(WalkerWorkingArea area);
}
