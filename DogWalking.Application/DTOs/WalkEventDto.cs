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

/// <summary>Walker responds to a walk request or proposal.
/// DeclineNote is required when Accepted = false and the walk was Proposed.</summary>
public record WalkerResponseDto(int WalkEventId, int WalkerId, bool Accepted, string? DeclineNote = null);

public record UpdateWalkStatusDto(int Id, WalkStatus NewStatus);

public record AssignWalkerDto(int WalkEventId, int? WalkerId);

/// <summary>Monthly summary of walk usage vs subscription limits.</summary>
public record MonthlyWalkSummaryDto(int ActiveCount, int MaxWalksPerMonth, int Remaining, string PlanDescription);

/// <summary>Generic paged result for server-side pagination.</summary>
public record PagedResultDto<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
