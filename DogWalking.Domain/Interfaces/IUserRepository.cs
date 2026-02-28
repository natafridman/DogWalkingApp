using DogWalking.Domain.Entities;

namespace DogWalking.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?>             GetByIdAsync(int id, CancellationToken ct = default);
    Task<User?>             GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<IEnumerable<User>> GetWalkersAsync(CancellationToken ct = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default);

    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
}
