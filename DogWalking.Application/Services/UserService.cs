using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Interfaces;

namespace DogWalking.Application.Services;

/// <summary>Provides user query operations for the UI layer.</summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;

    public UserService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<UserDto>> GetWalkersAsync(CancellationToken ct = default)
    {
        var walkers = await _uow.Users.GetWalkersAsync(ct);

        return walkers.Select(u => new UserDto(u.Id, u.Username, u.FullName, u.Role, u.IsActive, u.Phone, u.Email));
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _uow.Users.GetAllAsync(ct);

        return users.Select(u => new UserDto(u.Id, u.Username, u.FullName, u.Role, u.IsActive, u.Phone, u.Email));
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(id, ct);
        if (user is null) return null;

        return new UserDto(user.Id, user.Username, user.FullName, user.Role, user.IsActive, user.Phone, user.Email);
    }

    public async Task UpdateContactInfoAsync(int userId, string? phone, string? email, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        user.UpdateContactInfo(phone, email);

        _uow.Users.Update(user);
        await _uow.CommitAsync(ct);
    }
}
