using System.Security.Cryptography;
using System.Text;

using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Entities;
using DogWalking.Domain.Interfaces;

namespace DogWalking.Application.Services;

/// <summary>
/// Handles authentication. Passwords hashed with SHA-256 (demo).
/// Production should use BCrypt / Argon2.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;

    public AuthService(IUnitOfWork uow) => _uow = uow;

    public async Task<AuthResultDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return Fail("Username and password are required.");

        var user = await _uow.Users.GetByUsernameAsync(dto.Username.ToLowerInvariant(), ct);

        if (user is null || !user.IsActive)
            return Fail("Invalid credentials.");

        if (user.PasswordHash != HashPassword(dto.Password))
            return Fail("Invalid credentials.");

        return new AuthResultDto(true, null, user.Id, user.Username, user.FullName, user.Role.ToString());
    }

    public async Task<bool> CreateUserAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        var existing = await _uow.Users.GetByUsernameAsync(dto.Username.ToLowerInvariant(), ct);
        if (existing is not null) return false;

        var user = new User(dto.Username, HashPassword(dto.Password), dto.FullName, dto.Role,
                            dto.Phone, dto.Email);

        await _uow.Users.AddAsync(user, ct);
        await _uow.CommitAsync(ct);

        return true;
    }

    private static AuthResultDto Fail(string msg) =>
        new(false, msg, null, null, null, null);

    public static string HashPassword(string password) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
}