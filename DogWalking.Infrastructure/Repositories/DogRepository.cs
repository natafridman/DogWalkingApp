using DogWalking.Domain.Entities;
using DogWalking.Domain.Interfaces;
using DogWalking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DogWalking.Infrastructure.Repositories;

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
}
