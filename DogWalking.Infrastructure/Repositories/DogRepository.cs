using DogWalking.Domain.Entities;
using DogWalking.Domain.Interfaces;
using DogWalking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DogWalking.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IDogRepository.
/// Eager-loads Client on reads; GetByIdWithWalksAsync also loads WalkEvents
/// so the service layer can check for conflicts before scheduling.
/// </summary>
public class DogRepository : IDogRepository
{
    private readonly DogWalkingDbContext _ctx;

    public DogRepository(DogWalkingDbContext ctx) => _ctx = ctx;

    public async Task<Dog?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.Dogs
            .Include(d => d.Client)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IEnumerable<Dog>> GetByClientIdAsync(int clientId, CancellationToken ct = default)
        => await _ctx.Dogs
            .AsNoTracking()
            .Where(d => d.ClientId == clientId)
            .Include(d => d.Client)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);

    public async Task AddAsync(Dog dog, CancellationToken ct = default)
        => await _ctx.Dogs.AddAsync(dog, ct);

    public void Update(Dog dog) => _ctx.Dogs.Update(dog);
    public void Remove(Dog dog) => _ctx.Dogs.Remove(dog);

    public async Task<Dog?> GetByIdWithWalksAsync(int id, CancellationToken ct = default)
        => await _ctx.Dogs
            .Include(d => d.Client)
            .Include(d => d.WalkEvents)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
}
