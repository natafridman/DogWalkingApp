using DogWalking.Domain.Enums;
using DogWalking.Domain.Exceptions;
using DogWalking.Domain.Interfaces;

namespace DogWalking.Domain.Services;

/// <summary>
/// Factory Pattern: centralises creation of walk limit strategies.
/// Callers request a strategy by subscription type without knowing concrete types.
/// </summary>
public static class WalkLimitStrategyFactory
{
    /// <summary>Returns the appropriate walk limit strategy for the given subscription tier.</summary>
    public static IWalkLimitStrategy Create(SubscriptionType subscriptionType) =>
        subscriptionType switch
        {
            SubscriptionType.Free    => new FreeWalkLimitStrategy(),
            SubscriptionType.Basic   => new BasicWalkLimitStrategy(),
            SubscriptionType.Pro     => new ProWalkLimitStrategy(),
            SubscriptionType.Premium => new PremiumWalkLimitStrategy(),
            _ => throw new DomainException($"No walk limit strategy defined for subscription '{subscriptionType}'.")
        };

    /// <summary>Returns all available strategies — useful for UI display of subscription options.</summary>
    public static IEnumerable<IWalkLimitStrategy> GetAll() =>
    [
        new FreeWalkLimitStrategy(),
        new BasicWalkLimitStrategy(),
        new ProWalkLimitStrategy(),
        new PremiumWalkLimitStrategy()
    ];
}
