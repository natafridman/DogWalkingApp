namespace DogWalking.Application.DTOs;

/// <summary>One availability window for a walker (e.g., Monday 08:00–12:00, zone: Palermo).</summary>
public record WalkerAvailabilityDto(
    int       Id,
    int       WalkerId,
    string    WalkerName,
    DayOfWeek DayOfWeek,
    TimeOnly  StartTime,
    TimeOnly  EndTime,
    string    Zone
);

/// <summary>Represents a walker returned as eligible for a walk request.</summary>
public record WalkerMatchDto(int WalkerId, string WalkerName, IEnumerable<string> WorkingZones);

public record CreateAvailabilityDto(
    int       WalkerId,
    DayOfWeek DayOfWeek,
    TimeOnly  StartTime,
    TimeOnly  EndTime,
    string    Zone = ""
);
