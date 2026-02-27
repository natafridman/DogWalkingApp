using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Interfaces;
using DogWalking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DogWalking.Infrastructure.Repositories;

public class WalkEventRepository : IWalkEventRepository
{
    private readonly DogWalkingDbContext _ctx;

    public WalkEventRepository(DogWalkingDbContext ctx) => _ctx = ctx;

    public async Task<WalkEvent?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.WalkEvents
            .Include(w => w.Dog).ThenInclude(d => d.Client)
            .Include(w => w.Walker)
            .FirstOrDefaultAsync(w => w.Id == id, ct);

    public async Task<IEnumerable<WalkEvent>> GetByDogIdAsync(int dogId,
                                                               CancellationToken ct = default)
        => await _ctx.WalkEvents
            .AsNoTracking()
            .Where(w => w.DogId == dogId)
            .Include(w => w.Dog).ThenInclude(d => d.Client)
            .Include(w => w.Walker)
            .OrderByDescending(w => w.WalkDate)
            .ToListAsync(ct);

    public async Task<IEnumerable<WalkEvent>> GetByStatusAsync(WalkStatus status,
                                                                CancellationToken ct = default)
        => await _ctx.WalkEvents
            .AsNoTracking()
            .Where(w => w.Status == status)
            .Include(w => w.Dog).ThenInclude(d => d.Client)
            .Include(w => w.Walker)
            .Include(w => w.Declines)
            .OrderBy(w => w.WalkDate)
            .ToListAsync(ct);

    public async Task<IEnumerable<WalkEvent>> GetByDateRangeAsync(DateTime from, DateTime to,
                                                                   CancellationToken ct = default)
        => await _ctx.WalkEvents
            .AsNoTracking()
            .Where(w => w.WalkDate >= from && w.WalkDate <= to)
            .Include(w => w.Dog).ThenInclude(d => d.Client)
            .Include(w => w.Walker)
            .OrderBy(w => w.WalkDate)
            .ToListAsync(ct);

    /// <summary>
    /// Efficient query for subscription validation.
    /// Joins Dogs → Client to filter by clientId without loading all entities.
    /// </summary>
    public async Task<IEnumerable<WalkEvent>> GetByClientAndMonthAsync(
        int clientId, int year, int month, CancellationToken ct = default)
        => await _ctx.WalkEvents
            .AsNoTracking()
            .Where(w => w.Dog.ClientId == clientId
                        && w.WalkDate.Year  == year
                        && w.WalkDate.Month == month)
            .ToListAsync(ct);

    /// <summary>
    /// Server-side projection: counts only active walks without materializing entities.
    /// </summary>
    public async Task<int> CountActiveByClientAndMonthAsync(
        int clientId, int year, int month, CancellationToken ct = default)
        => await _ctx.WalkEvents
            .AsNoTracking()
            .Where(w => w.Dog.ClientId == clientId
                        && w.WalkDate.Year  == year
                        && w.WalkDate.Month == month
                        && (w.Status == WalkStatus.Requested || w.Status == WalkStatus.Proposed
                         || w.Status == WalkStatus.Accepted  || w.Status == WalkStatus.InProgress))
            .CountAsync(ct);

    /// <summary>
    /// Server-side pagination with total count — avoids loading all rows into memory.
    /// </summary>
    public async Task<(IEnumerable<WalkEvent> Items, int TotalCount)> GetByStatusPagedAsync(
        WalkStatus status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _ctx.WalkEvents
            .AsNoTracking()
            .Where(w => w.Status == status)
            .Include(w => w.Dog).ThenInclude(d => d.Client)
            .Include(w => w.Walker)
            .OrderBy(w => w.WalkDate);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IEnumerable<WalkEvent>> GetByWalkerIdAsync(int walkerId,
                                                                   CancellationToken ct = default)
        => await _ctx.WalkEvents
            .AsNoTracking()
            .Where(w => w.WalkerId == walkerId)
            .Include(w => w.Dog).ThenInclude(d => d.Client)
            .Include(w => w.Walker)
            .OrderBy(w => w.WalkDate)
            .ToListAsync(ct);

    public async Task<IEnumerable<WalkEvent>> GetByClientIdAsync(int clientId,
                                                                  CancellationToken ct = default)
        => await _ctx.WalkEvents
            .AsNoTracking()
            .Where(w => w.Dog.ClientId == clientId)
            .Include(w => w.Dog).ThenInclude(d => d.Client)
            .Include(w => w.Walker)
            .OrderBy(w => w.WalkDate)
            .ToListAsync(ct);

    public async Task AddAsync(WalkEvent walkEvent, CancellationToken ct = default)
        => await _ctx.WalkEvents.AddAsync(walkEvent, ct);

    public async Task AddRangeAsync(IEnumerable<WalkEvent> walkEvents, CancellationToken ct = default)
        => await _ctx.WalkEvents.AddRangeAsync(walkEvents, ct);

    public void Update(WalkEvent walkEvent) => _ctx.WalkEvents.Update(walkEvent);
    public void Remove(WalkEvent walkEvent) => _ctx.WalkEvents.Remove(walkEvent);
}
