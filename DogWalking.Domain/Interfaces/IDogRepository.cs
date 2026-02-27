using DogWalking.Domain.Entities;

namespace DogWalking.Domain.Interfaces;

/// <summary>
/// Persistence contract for the Dog entity.
/// GetByIdWithWalksAsync eagerly loads walk events — needed for overlap validation.
/// </summary>
public interface IDogRepository
{
    Task<Dog?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Dog>> GetByClientIdAsync(int clientId, CancellationToken ct = default);
    Task AddAsync(Dog dog, CancellationToken ct = default);
    void Update(Dog dog);
    void Remove(Dog dog);

    /// <summary>Loads the dog with its walk events for conflict checking before scheduling.</summary>
    Task<Dog?> GetByIdWithWalksAsync(int id, CancellationToken ct = default);
}
