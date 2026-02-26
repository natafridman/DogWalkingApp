namespace DogWalking.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IClientRepository Clients { get; }
    Task<int> CommitAsync(CancellationToken ct = default);
}