using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;

namespace DogWalking.Domain.Services;

/// <summary>
/// Pure domain service: finds walkers eligible to handle a specific walk request.
/// A walker is eligible when ALL criteria are met:
///   1. They have an availability slot on that day, in the right zone, that covers the walk duration.
///   2. They have no conflicting (Proposed/Accepted/InProgress) walk at that time.
/// </summary>
public static class WalkMatchingService
{
    /// <summary>
    /// Returns the set of walker IDs eligible to handle <paramref name="walkRequest"/>.
    /// Pass all availability slots and existing walks for candidates.
    /// Zone eligibility is determined per-slot via <see cref="WalkerAvailability.IsInZone"/>.
    /// </summary>
    public static IEnumerable<int> FindEligibleWalkers(
        WalkEvent                       walkRequest,
        IEnumerable<WalkerAvailability> allAvailabilities,
        IEnumerable<WalkEvent>          existingWalks)
    {
        var localDate   = walkRequest.WalkDate.ToLocalTime();
        var walkDay     = localDate.DayOfWeek;
        var walkTime    = TimeOnly.FromDateTime(localDate);
        var location    = walkRequest.Location;
        var proposedEnd = walkRequest.WalkDate.AddMinutes(walkRequest.DurationMinutes);

        // Criterion 1 — slot covers day + time + zone
        var availableWalkers = allAvailabilities
            .Where(a => a.DayOfWeek == walkDay
                     && a.CoversWalk(walkTime, walkRequest.DurationMinutes)
                     && a.IsInZone(location))
            .Select(a => a.WalkerId)
            .ToHashSet();

        // Criterion 2 — walker has no conflicting active walk at the same time
        var conflictingWalkers = existingWalks
            .Where(w => w.WalkerId.HasValue
                     && w.Status is WalkStatus.Proposed or WalkStatus.Accepted or WalkStatus.InProgress)
            .Where(w =>
            {
                var existingEnd = w.WalkDate.AddMinutes(w.DurationMinutes);
                return walkRequest.WalkDate < existingEnd && proposedEnd > w.WalkDate;
            })
            .Select(w => w.WalkerId!.Value)
            .ToHashSet();

        return availableWalkers.Except(conflictingWalkers);
    }
}
