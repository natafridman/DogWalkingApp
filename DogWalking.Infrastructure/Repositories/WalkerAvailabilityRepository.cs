using DogWalking.Domain.Entities;
using DogWalking.Domain.Interfaces;
using DogWalking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DogWalking.Infrastructure.Repositories;

public class WalkerAvailabilityRepository : IWalkerAvailabilityRepository
{
    private readonly DogWalkingDbContext _ctx;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public WalkerAvailabilityRepository(DogWalkingDbContext ctx, IMemoryCache cache)
    {
        _ctx = ctx;
        _cache = cache;
    }

    private static string CacheKey(int walkerId) => $"avail:{walkerId}";

    public async Task<WalkerAvailability?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.WalkerAvailabilities.FindAsync([id], ct);

    public async Task<IEnumerable<WalkerAvailability>> GetByWalkerIdAsync(int walkerId, CancellationToken ct = default)
    {
        var key = CacheKey(walkerId);
        if (_cache.TryGetValue(key, out IEnumerable<WalkerAvailability>? cached) && cached is not null)
            return cached;

        var result = await _ctx.WalkerAvailabilities.AsNoTracking()
                         .Where(a => a.WalkerId == walkerId)
                         .OrderBy(a => a.DayOfWeek).ThenBy(a => a.StartTime)
                         .ToListAsync(ct);

        _cache.Set(key, (IEnumerable<WalkerAvailability>)result, CacheDuration);
        return result;
    }

    public async Task<IEnumerable<WalkerAvailability>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.WalkerAvailabilities.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(WalkerAvailability availability, CancellationToken ct = default)
    {
        await _ctx.WalkerAvailabilities.AddAsync(availability, ct);
        _cache.Remove(CacheKey(availability.WalkerId));
    }

    public void Update(WalkerAvailability availability)
    {
        _ctx.WalkerAvailabilities.Update(availability);
        _cache.Remove(CacheKey(availability.WalkerId));
    }

    public void Remove(WalkerAvailability availability)
    {
        _ctx.WalkerAvailabilities.Remove(availability);
        _cache.Remove(CacheKey(availability.WalkerId));
    }
}
