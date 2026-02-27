using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Exceptions;
using DogWalking.Domain.Interfaces;

namespace DogWalking.Domain.Services;

/// <summary>
/// Abstract base for walk limit strategies.
/// Implements shared validation logic; subclasses configure the parameters.
/// </summary>
public abstract class WalkLimitStrategyBase : IWalkLimitStrategy
{
    public abstract SubscriptionType SubscriptionType { get; }
    public abstract int MaxWalksPerMonth { get; }
    public abstract bool AllowsMultiplePerDay { get; }
    public abstract string Description { get; }

    public void ValidateWalkAllowed(IEnumerable<WalkEvent> existingWalksThisMonth, DateTime proposedDate)
    {
        // Only count walks that are still "in-flight" against the limit.
        // Cancelled and Completed walks should not block new requests.
        var activeWalks = existingWalksThisMonth
            .Where(w => w.Status is WalkStatus.Requested or WalkStatus.Proposed
                                 or WalkStatus.Accepted  or WalkStatus.InProgress)
            .ToList();

        if (activeWalks.Count >= MaxWalksPerMonth)
            throw new DomainException(
                $"Subscription limit reached. {SubscriptionType} allows up to {MaxWalksPerMonth} walks per month " +
                $"({activeWalks.Count} already used).");

        if (!AllowsMultiplePerDay)
        {
            var proposedDay = proposedDate.Date;
            bool alreadyHasWalkThatDay = activeWalks
                .Any(w => w.WalkDate.Date == proposedDay);

            if (alreadyHasWalkThatDay)
                throw new DomainException(
                    $"Subscription {SubscriptionType} allows only one walk per day. " +
                    $"A walk is already scheduled for {proposedDay:yyyy-MM-dd}.");
        }
    }
}
