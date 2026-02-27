using DogWalking.Domain.Entities;

namespace DogWalking.Domain.Interfaces;

/// <summary>
/// Persistence contract for walker time-slot availability.
/// Results are cached at the infrastructure level for frequent reads.
/// </summary>
public interface IWalkerAvailabilityRepository
{
    Task<WalkerAvailability?>             GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<WalkerAvailability>> GetByWalkerIdAsync(int walkerId, CancellationToken ct = default);
    Task<IEnumerable<WalkerAvailability>> GetAllAsync(CancellationToken ct = default);
    Task                                  AddAsync(WalkerAvailability availability, CancellationToken ct = default);
    void                                  Update(WalkerAvailability availability);
    void                                  Remove(WalkerAvailability availability);
}
