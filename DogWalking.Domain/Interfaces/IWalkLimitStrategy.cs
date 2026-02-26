using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;

namespace DogWalking.Domain.Interfaces;

/// <summary>
/// Strategy Pattern: defines the contract for subscription-based walk limit validation.
/// Each subscription tier implements its own rule set.
/// </summary>
public interface IWalkLimitStrategy
{
    SubscriptionType SubscriptionType { get; }
    int MaxWalksPerMonth { get; }
    bool AllowsMultiplePerDay { get; }
    string Description { get; }

    /// <summary>
    /// Validates whether a new walk can be scheduled under this subscription.
    /// Throws <see cref="Exceptions.DomainException"/> if the limit is exceeded.
    /// </summary>
    /// <param name="existingWalksThisMonth">
    /// All non-cancelled walks for the client in the proposed walk's month.
    /// </param>
    /// <param name="proposedDate">The UTC date/time of the new walk.</param>
    void ValidateWalkAllowed(IEnumerable<WalkEvent> existingWalksThisMonth, DateTime proposedDate);
}
