namespace DogWalking.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    Task<int> CommitAsync(CancellationToken ct = default);
}