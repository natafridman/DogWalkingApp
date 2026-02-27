using DogWalking.Domain.Entities;
using DogWalking.Domain.Interfaces;
using DogWalking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DogWalking.Infrastructure.Repositories;

public class WalkerAvailabilityRepository : IWalkerAvailabilityRepository
{
    private readonly DogWalkingDbContext _ctx;

    public WalkerAvailabilityRepository(DogWalkingDbContext ctx) => _ctx = ctx;

    public async Task<WalkerAvailability?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.WalkerAvailabilities.FindAsync([id], ct);

    public async Task<IEnumerable<WalkerAvailability>> GetByWalkerIdAsync(int walkerId, CancellationToken ct = default)
        => await _ctx.WalkerAvailabilities.AsNoTracking()
                     .Where(a => a.WalkerId == walkerId)
                     .OrderBy(a => a.DayOfWeek).ThenBy(a => a.StartTime)
                     .ToListAsync(ct);

    public async Task<IEnumerable<WalkerAvailability>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.WalkerAvailabilities.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(WalkerAvailability availability, CancellationToken ct = default)
        => await _ctx.WalkerAvailabilities.AddAsync(availability, ct);

    public void Update(WalkerAvailability availability)
        => _ctx.WalkerAvailabilities.Update(availability);

    public void Remove(WalkerAvailability availability)
        => _ctx.WalkerAvailabilities.Remove(availability);
}
