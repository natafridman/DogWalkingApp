namespace DogWalking.Domain.Interfaces;

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