using DogWalking.Domain.Enums;

namespace DogWalking.Application.DTOs;

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
