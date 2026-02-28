using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;

namespace DogWalking.Domain.Services;

/// <summary>
/// Finds walkers that match a walk request by checking availability and schedule conflicts.
/// </summary>
public static class WalkMatchingService
{
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
