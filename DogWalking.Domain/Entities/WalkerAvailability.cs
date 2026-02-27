using DogWalking.Domain.Exceptions;

namespace DogWalking.Domain.Entities;

/// <summary>
/// Represents a recurring availability window for a walker on a specific day of the week,
/// tied to a specific geographic zone. Both time coverage and zone must match for the walker
/// to be considered eligible for a walk request.
/// </summary>
public class WalkerAvailability
{
    public int       Id        { get; private set; }
    public int       WalkerId  { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly  StartTime { get; private set; }
    public TimeOnly  EndTime   { get; private set; }
    public string    Zone      { get; private set; } = string.Empty;

    public User Walker { get; private set; } = null!;

    // Required by EF Core
    private WalkerAvailability() { }

    public WalkerAvailability(int walkerId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime,
                               string zone = "")
    {
        WalkerId  = walkerId;
        DayOfWeek = dayOfWeek;
        Zone      = zone.Trim();
        SetTimes(startTime, endTime);
    }

    public void Update(DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime, string zone = "")
    {
        DayOfWeek = dayOfWeek;
        Zone      = zone.Trim();
        SetTimes(startTime, endTime);
    }

    /// <summary>
    /// Returns true if a walk starting at <paramref name="walkTime"/> and lasting
    /// <paramref name="durationMinutes"/> falls entirely within this availability window.
    /// </summary>
    public bool CoversWalk(TimeOnly walkTime, int durationMinutes)
    {
        var walkEnd = walkTime.AddMinutes(durationMinutes);
        return walkTime >= StartTime && walkEnd <= EndTime;
    }

    /// <summary>
    /// Returns true if this slot's zone matches the given walk location
    /// (bidirectional substring, case-insensitive).
    /// </summary>
    public bool IsInZone(string location) =>
        !string.IsNullOrWhiteSpace(Zone) &&
        (location.Contains(Zone, StringComparison.OrdinalIgnoreCase) ||
         Zone.Contains(location, StringComparison.OrdinalIgnoreCase));

    private void SetTimes(TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
            throw new DomainException("End time must be after start time.");
        StartTime = startTime;
        EndTime   = endTime;
    }
}
