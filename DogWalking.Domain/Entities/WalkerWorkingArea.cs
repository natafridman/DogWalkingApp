using DogWalking.Domain.Exceptions;

namespace DogWalking.Domain.Entities;

/// <summary>
/// Represents a geographic zone/neighbourhood where a walker is available to work.
/// Walk requests are matched to walkers whose working areas include the walk's location.
/// </summary>
public class WalkerWorkingArea
{
    public int    Id       { get; private set; }
    public int    WalkerId { get; private set; }
    public string ZoneName { get; private set; } = string.Empty;

    public User Walker { get; private set; } = null!;

    // Required by EF Core
    private WalkerWorkingArea() { }

    public WalkerWorkingArea(int walkerId, string zoneName)
    {
        WalkerId = walkerId;
        SetZoneName(zoneName);
    }

    private void SetZoneName(string zoneName)
    {
        if (string.IsNullOrWhiteSpace(zoneName))
            throw new DomainException("Zone name cannot be empty.");
        if (zoneName.Length > 100)
            throw new DomainException("Zone name cannot exceed 100 characters.");
        ZoneName = zoneName.Trim();
    }
}
