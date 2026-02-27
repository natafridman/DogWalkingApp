using DogWalking.Domain.Entities;
using DogWalking.Domain.Interfaces;
using DogWalking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DogWalking.Infrastructure.Repositories;

public class WalkerWorkingAreaRepository : IWalkerWorkingAreaRepository
{
    private readonly DogWalkingDbContext _ctx;

    public WalkerWorkingAreaRepository(DogWalkingDbContext ctx) => _ctx = ctx;

    public async Task<WalkerWorkingArea?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.WalkerWorkingAreas.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IEnumerable<WalkerWorkingArea>> GetByWalkerIdAsync(int walkerId, CancellationToken ct = default)
        => await _ctx.WalkerWorkingAreas.AsNoTracking()
                     .Where(a => a.WalkerId == walkerId)
                     .OrderBy(a => a.ZoneName)
                     .ToListAsync(ct);

    public async Task<IEnumerable<WalkerWorkingArea>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.WalkerWorkingAreas.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(WalkerWorkingArea area, CancellationToken ct = default)
        => await _ctx.WalkerWorkingAreas.AddAsync(area, ct);

    public void Remove(WalkerWorkingArea area)
        => _ctx.WalkerWorkingAreas.Remove(area);
}
