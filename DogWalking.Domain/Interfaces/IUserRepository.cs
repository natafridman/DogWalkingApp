using DogWalking.Domain.Entities;

namespace DogWalking.Domain.Interfaces;

/// <summary>
/// Persistence contract for User (authentication and profile data).
/// </summary>
public interface IUserRepository
{
    Task<User?>             GetByIdAsync(int id, CancellationToken ct = default);
    Task<User?>             GetByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>Returns all active users with role Walker — used to populate walker dropdowns.</summary>
    Task<IEnumerable<User>> GetWalkersAsync(CancellationToken ct = default);

    /// <summary>Returns all users — used by the Admin Users tab.</summary>
    Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default);

    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
}
