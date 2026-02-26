using DogWalking.Application.DTOs;

namespace DogWalking.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<bool> CreateUserAsync(CreateUserDto dto, CancellationToken ct = default);
    Task<AuthResultDto> RegisterClientUserAsync(RegisterClientUserDto dto, CancellationToken ct = default);
}
