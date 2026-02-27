using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Interfaces;
using DogWalking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DogWalking.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IUserRepository.
/// Read-only queries use AsNoTracking for performance.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly DogWalkingDbContext _ctx;

    public UserRepository(DogWalkingDbContext ctx) => _ctx = ctx;

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.Users.FindAsync(new object[] { id }, ct);

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await _ctx.Users
            .FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<IEnumerable<User>> GetWalkersAsync(CancellationToken ct = default)
        => await _ctx.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Walker && u.IsActive)
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Users
            .AsNoTracking()
            .OrderBy(u => u.Role).ThenBy(u => u.FullName)
            .ToListAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _ctx.Users.AddAsync(user, ct);

    public void Update(User user) 
        => _ctx.Users.Update(user);
}
