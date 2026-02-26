using DogWalking.Domain.Interfaces;

namespace DogWalking.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    Task<int> CommitAsync(CancellationToken ct = default);
}