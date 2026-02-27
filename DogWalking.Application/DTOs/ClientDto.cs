using DogWalking.Domain.Enums;

namespace DogWalking.Application.DTOs;

/// <summary>Read model for clients — includes computed DogCount for admin grid display.</summary>
public record ClientDto(
    int Id,
    string Name,
    string PhoneNumber,
    string Email,
    SubscriptionType Subscription,
    bool IsActive,
    int DogCount,
    string Zone,
    string Address = ""
);

/// <summary>Input model for creating a new client (admin or registration flow).</summary>
public record CreateClientDto(
    string Name,
    string PhoneNumber,
    string Email,
    SubscriptionType Subscription = SubscriptionType.Free,
    string Zone = "",
    string Address = ""
);

public record UpdateClientDto(
    int Id,
    string Name,
    string PhoneNumber,
    string Email,
    SubscriptionType Subscription,
    string Zone = ""
);

public record ChangeSubscriptionDto(int ClientId, SubscriptionType NewSubscription);
