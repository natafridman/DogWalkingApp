using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;

namespace DogWalking.Domain.Interfaces;

public interface IWalkLimitStrategy
{
    SubscriptionType SubscriptionType { get; }
    int MaxWalksPerMonth { get; }
    bool AllowsMultiplePerDay { get; }
    string Description { get; }

    /// <summary>Throws DomainException if the subscription limit is exceeded.</summary>
    void ValidateWalkAllowed(IEnumerable<WalkEvent> existingWalksThisMonth, DateTime proposedDate);
}
