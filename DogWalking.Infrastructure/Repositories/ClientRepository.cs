using DogWalking.Domain.Entities;
using DogWalking.Domain.Interfaces;
using DogWalking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DogWalking.Infrastructure.Repositories;

/// <summary>
/// Repository Pattern implementation for Client.
/// AsNoTracking on read-only queries → better EF Core performance.
/// IMemoryCache for subscription/profile lookups that rarely change.
/// </summary>
public class ClientRepository : IClientRepository
{
    private readonly DogWalkingDbContext _ctx;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public ClientRepository(DogWalkingDbContext ctx, IMemoryCache cache)
    {
        _ctx = ctx;
        _cache = cache;
    }

    private static string CacheKey(int id) => $"client:{id}";

    public async Task<Client?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var key = CacheKey(id);
        if (_cache.TryGetValue(key, out Client? cached) && cached is not null)
            return cached;

        var client = await _ctx.Clients.FindAsync(new object[] { id }, ct);
        if (client is not null)
            _cache.Set(key, client, CacheDuration);
        return client;
    }

    public async Task<Client?> GetByIdWithDogsAsync(int id, CancellationToken ct = default)
        => await _ctx.Clients
            .Include(c => c.Dogs)
                .ThenInclude(d => d.WalkEvents)
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

    public void Update(Client client)
    {
        _ctx.Clients.Update(client);
        _cache.Remove(CacheKey(client.Id));
    }

    public void Remove(Client client)
    {
        _ctx.Clients.Remove(client);
        _cache.Remove(CacheKey(client.Id));
    }
}
