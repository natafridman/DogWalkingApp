using DogWalking.Domain.Entities;
using DogWalking.Domain.Interfaces;
using DogWalking.Infrastructure.Data;

namespace DogWalking.Infrastructure.Repositories;

/// <summary>
/// Repository Pattern implementation for Client.
/// AsNoTracking on read-only queries → better EF Core performance.
/// Tracking only for entities that will be modified.
/// </summary>
public class ClientRepository : IClientRepository
{
    private readonly DogWalkingDbContext _ctx;

    public ClientRepository(DogWalkingDbContext ctx) => _ctx = ctx;

    public async Task<Client?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.Clients.FindAsync([id], ct);

    public async Task AddAsync(Client client, CancellationToken ct = default)
        => await _ctx.Clients.AddAsync(client, ct);

    public void Update(Client client) => _ctx.Clients.Update(client);
    public void Remove(Client client) => _ctx.Clients.Remove(client);
}
