using DogWalking.Domain.Enums;

namespace DogWalking.Application.DTOs;

public record LoginDto(string Username, string Password);

public record AuthResultDto(
    bool Success,
    string? ErrorMessage,
    int? UserId,
    string? Username,
    string? FullName,
    string? Role
);

public record UserDto(int Id, string Username, string FullName, UserRole Role, bool IsActive,
                      string? Phone = null, string? Email = null);

public record CreateUserDto(string Username, string Password, string FullName, UserRole Role,
                            string? Phone = null, string? Email = null);

public record RegisterClientUserDto(
    string Username,
    string Password,
    string FullName,
    string PhoneNumber,
    string Email,
    SubscriptionType Subscription,
    string Address = "",
    string ConfirmPassword = ""
);
