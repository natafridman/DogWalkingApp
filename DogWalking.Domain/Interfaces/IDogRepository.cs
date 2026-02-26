using DogWalking.Domain.Entities;

namespace DogWalking.Domain.Interfaces;

public interface IDogRepository
{
    Task<Dog?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Dog>> GetByClientIdAsync(int clientId, CancellationToken ct = default);
    Task AddAsync(Dog dog, CancellationToken ct = default);
    void Update(Dog dog);
    void Remove(Dog dog);
    Task<Dog?> GetByIdWithWalksAsync(int id, CancellationToken ct = default);
}
