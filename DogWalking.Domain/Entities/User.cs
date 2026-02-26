using DogWalking.Domain.Enums;
using DogWalking.Domain.Exceptions;

namespace DogWalking.Domain.Entities;

/// <summary>
/// Application user.  Role determines what screens and actions are available.
/// Passwords are stored as hashes — never in plain text.
/// </summary>
public class User
{
    public int      Id           { get; private set; }
    public string   Username     { get; private set; } = string.Empty;
    public string   PasswordHash { get; private set; } = string.Empty;
    public UserRole Role         { get; private set; }
    public string   FullName     { get; private set; } = string.Empty;
    public string?  Phone        { get; private set; }
    public string?  Email        { get; private set; }
    public bool     IsActive     { get; private set; }
    public DateTime CreatedAt    { get; private set; }

    // Required by EF Core
    private User() { }

    public User(string username, string passwordHash, string fullName, UserRole role = UserRole.Client,
                string? phone = null, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("Username cannot be empty.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash cannot be empty.");
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name cannot be empty.");

        Username     = username.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        FullName     = fullName.Trim();
        Role         = role;
        Phone        = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Email        = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        IsActive     = true;
        CreatedAt    = DateTime.UtcNow;
    }

    public void UpdatePasswordHash(string newHash)
    {
        if (string.IsNullOrWhiteSpace(newHash))
            throw new DomainException("Password hash cannot be empty.");
        PasswordHash = newHash;
    }

    public void UpdateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name cannot be empty.");
        FullName = fullName.Trim();
    }

    public void UpdateContactInfo(string? phone, string? email)
    {
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }

    public void Deactivate() => IsActive = false;
    public void Activate()   => IsActive = true;
}
