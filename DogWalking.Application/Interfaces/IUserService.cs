using DogWalking.Application.DTOs;

namespace DogWalking.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetWalkersAsync(CancellationToken ct = default);
    Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task UpdateContactInfoAsync(int userId, string? phone, string? email, CancellationToken ct = default);
}
