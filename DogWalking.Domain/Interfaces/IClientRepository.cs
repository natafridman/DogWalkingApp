using DogWalking.Domain.Entities;

namespace DogWalking.Domain.Interfaces;

/// <summary>
/// Repository Pattern: persistence contract for the Client aggregate.
/// Defined in Domain so Application depends on abstractions, not EF Core.
/// </summary>
public interface IClientRepository
{
    Task<Client?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Client client, CancellationToken ct = default);
    void Update(Client client);
    void Remove(Client client);
    Task<Client?> GetByIdWithDogsAsync(int id, CancellationToken ct = default);
    Task<Client?> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task<IEnumerable<Client>> GetAllActiveAsync(CancellationToken ct = default);
    Task<IEnumerable<Client>> SearchAsync(string term, CancellationToken ct = default);
}