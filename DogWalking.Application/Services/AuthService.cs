using System.Security.Cryptography;
using System.Text;
using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Interfaces;
using FluentValidation;

namespace DogWalking.Application.Services;

/// <summary>
/// Handles authentication. Passwords hashed with SHA-256 (demo).
/// Production should use BCrypt / Argon2.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IValidator<CreateUserDto> _createUserValidator;
    private readonly IValidator<RegisterClientUserDto> _registerValidator;

    public AuthService(IUnitOfWork uow,
                       IValidator<LoginDto> loginValidator,
                       IValidator<CreateUserDto> createUserValidator,
                       IValidator<RegisterClientUserDto> registerValidator)
    {
        _uow = uow;
        _loginValidator = loginValidator;
        _createUserValidator = createUserValidator;
        _registerValidator = registerValidator;
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var validation = await _loginValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Fail(validation.Errors.First().ErrorMessage);

        var user = await _uow.Users.GetByUsernameAsync(dto.Username.ToLowerInvariant(), ct);

        if (user is null || !user.IsActive)
            return Fail("Invalid credentials.");

        if (user.PasswordHash != HashPassword(dto.Password))
            return Fail("Invalid credentials.");

        return new AuthResultDto(true, null, user.Id, user.Username, user.FullName, user.Role.ToString());
    }

    public async Task<bool> CreateUserAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        await _createUserValidator.ValidateAndThrowAsync(dto, ct);

        var existing = await _uow.Users.GetByUsernameAsync(dto.Username.ToLowerInvariant(), ct);
        if (existing is not null) return false;

        var user = new User(dto.Username, HashPassword(dto.Password), dto.FullName, dto.Role,
                            dto.Phone, dto.Email);
        await _uow.Users.AddAsync(user, ct);
        await _uow.CommitAsync(ct);
        return true;
    }

    public async Task<AuthResultDto> RegisterClientUserAsync(RegisterClientUserDto dto, CancellationToken ct = default)
    {
        await _registerValidator.ValidateAndThrowAsync(dto, ct);

        var existing = await _uow.Users.GetByUsernameAsync(dto.Username.ToLowerInvariant(), ct);
        if (existing is not null)
            return Fail("Username is already taken.");

        var user = new User(dto.Username, HashPassword(dto.Password), dto.FullName, UserRole.Client);
        await _uow.Users.AddAsync(user, ct);
        await _uow.CommitAsync(ct);

        var client = new Client(dto.FullName, dto.PhoneNumber, dto.Email, dto.Subscription,
                                userId: user.Id, address: dto.Address);
        await _uow.Clients.AddAsync(client, ct);
        await _uow.CommitAsync(ct);

        return new AuthResultDto(true, null, user.Id, user.Username, user.FullName, user.Role.ToString());
    }

    private static AuthResultDto Fail(string msg) =>
        new(false, msg, null, null, null, null);

    public static string HashPassword(string password) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
}