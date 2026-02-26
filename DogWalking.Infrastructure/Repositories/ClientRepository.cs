using DogWalking.Domain.Entities;
using DogWalking.Domain.Interfaces;
using DogWalking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
        => await _ctx.Clients.FindAsync(new object[] { id }, ct);

    public async Task<Client?> GetByIdWithDogsAsync(int id, CancellationToken ct = default)
        => await _ctx.Clients
            .Include(c => c.Dogs)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Client?> GetByUserIdAsync(int userId, CancellationToken ct = default)
        => await _ctx.Clients
            .AsNoTracking()
            .Include(c => c.Dogs)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive, ct);

    public async Task<IEnumerable<Client>> GetAllActiveAsync(CancellationToken ct = default)
        => await _ctx.Clients
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Include(c => c.Dogs)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<IEnumerable<Client>> SearchAsync(string term, CancellationToken ct = default)
        => await _ctx.Clients
            .AsNoTracking()
            .Where(c => c.IsActive &&
                        (EF.Functions.Like(c.Name, $"%{term}%") ||
                         EF.Functions.Like(c.Email, $"%{term}%")))
            .Include(c => c.Dogs)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task AddAsync(Client client, CancellationToken ct = default)
        => await _ctx.Clients.AddAsync(client, ct);


    public void Update(Client client) => _ctx.Clients.Update(client);
    public void Remove(Client client) => _ctx.Clients.Remove(client);
}
