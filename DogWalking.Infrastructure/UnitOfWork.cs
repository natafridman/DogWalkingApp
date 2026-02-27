using DogWalking.Domain.Exceptions;
using DogWalking.Domain.Interfaces;
using DogWalking.Infrastructure.Data;
using DogWalking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DogWalking.Infrastructure;

/// <summary>
/// Unit of Work Pattern implementation.
/// All repositories share the same DbContext instance,
/// ensuring they participate in the same transaction.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DogWalkingDbContext _ctx;

    public IUserRepository Users { get; }
    public IClientRepository Clients { get; }
    public IDogRepository Dogs { get; }
    public IWalkEventRepository WalkEvents { get; }
    public IWalkerAvailabilityRepository WalkerAvailabilities { get; }
    public IWalkerWorkingAreaRepository  WalkerWorkingAreas   { get; }

    public UnitOfWork(DogWalkingDbContext ctx)
    {
        _ctx = ctx;
        Users = new UserRepository(ctx);
        Clients = new ClientRepository(ctx);
        Dogs = new DogRepository(ctx);
        WalkEvents = new WalkEventRepository(ctx);
        WalkerAvailabilities = new WalkerAvailabilityRepository(ctx);
        WalkerWorkingAreas   = new WalkerWorkingAreaRepository(ctx);
    }

    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        try
        {
            return await _ctx.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException(
                "This record was modified by another user. Please refresh and try again.", ex);
        }
    }

    public void Dispose() => _ctx.Dispose();
}
