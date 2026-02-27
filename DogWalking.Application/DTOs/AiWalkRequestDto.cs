using DogWalking.Domain.Enums;

namespace DogWalking.Application.DTOs;

/// <summary>
/// Result of AI-powered natural language walk request parsing.
/// Contains new dogs to create and walk schedule entries to schedule.
/// </summary>
public record AiParsedWalkRequest(
    IReadOnlyList<AiParsedDog> NewDogs,
    IReadOnlyList<AiParsedWalk> Walks);

/// <summary>Dog that needs to be created before scheduling walks.</summary>
public record AiParsedDog(string Name, string Breed);

/// <summary>Individual walk entry parsed from the AI response.</summary>
public record AiParsedWalk(
    string DogName,
    DayOfWeek? DayOfWeek,
    TimeOnly Time,
    int DurationMinutes,
    string Location,
    RecurrenceType Recurrence);
