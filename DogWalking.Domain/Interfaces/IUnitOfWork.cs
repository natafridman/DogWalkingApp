namespace DogWalking.Domain.Interfaces;

/// <summary>
/// Groups all repositories under a single transaction.
/// Call CommitAsync once to persist changes from any combination of repositories.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IClientRepository Clients { get; }
    IDogRepository Dogs { get; }
    IWalkEventRepository WalkEvents { get; }
    IWalkerAvailabilityRepository WalkerAvailabilities { get; }
    IWalkerWorkingAreaRepository  WalkerWorkingAreas   { get; }

    Task<int> CommitAsync(CancellationToken ct = default);
}