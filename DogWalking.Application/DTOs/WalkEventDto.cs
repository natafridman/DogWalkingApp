using DogWalking.Domain.Enums;

namespace DogWalking.Application.DTOs;

public record WalkEventDto(
    int            Id,
    int            DogId,
    string         DogName,
    string         ClientName,
    int?           WalkerId,
    string?        WalkerName,
    DateTime       WalkDate,
    int            DurationMinutes,
    WalkStatus     Status,
    string         Location,
    DateTime?      EstimatedArrivalTime,
    string?        Notes,
    RecurrenceType RecurrenceType = RecurrenceType.OneTime,
    string         ClientAddress  = ""
);

/// <summary>DTO for a client to request a new walk (starts in Requested status).</summary>
public record CreateWalkEventDto(
    int            DogId,
    DateTime       WalkDate,
    int            DurationMinutes,
    string         Location,
    string?        Notes          = null,
    RecurrenceType RecurrenceType = RecurrenceType.OneTime
);

/// <summary>Admin/system proposes a specific walker for a walk request.</summary>
public record ProposeWalkDto(int WalkEventId, int WalkerId, DateTime? EstimatedArrival = null);

/// <summary>Walker responds to a proposal — accepted or rejected.
/// RejectionNote is required when Accepted = false and the walk was Proposed.</summary>
public record WalkerResponseDto(int WalkEventId, bool Accepted, string? RejectionNote = null);

public record UpdateWalkStatusDto(int Id, WalkStatus NewStatus);

public record AssignWalkerDto(int WalkEventId, int? WalkerId);
