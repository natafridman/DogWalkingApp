namespace DogWalking.Domain.Enums;

/// <summary>
/// Application user roles.
/// Admin  : full system access.
/// Walker : the person who physically walks dogs.
/// Client : the dog owner who purchases subscriptions and books walks.
/// </summary>
public enum UserRole
{
    Admin  = 1,
    Walker = 2,
    Client = 3
}
