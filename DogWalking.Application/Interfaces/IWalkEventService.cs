using DogWalking.Application.DTOs;
using DogWalking.Domain.Enums;

namespace DogWalking.Application.Interfaces;

public interface IWalkEventService
{
    Task<IEnumerable<WalkEventDto>> GetByDogIdAsync(int dogId, CancellationToken ct = default);
    Task<IEnumerable<WalkEventDto>> GetByStatusAsync(WalkStatus status, CancellationToken ct = default);
    Task<IEnumerable<WalkEventDto>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<IEnumerable<WalkEventDto>> GetByWalkerIdAsync(int walkerId, CancellationToken ct = default);
    Task<IEnumerable<WalkEventDto>> GetByClientIdAsync(int clientId, CancellationToken ct = default);

    /// <summary>Walk proposals pending the given walker's response (status = Proposed).</summary>
    Task<IEnumerable<WalkEventDto>> GetProposedForWalkerAsync(int walkerId, CancellationToken ct = default);

    /// <summary>All Requested walks whose location and time match this walker's zones/availability.</summary>
    Task<IEnumerable<WalkEventDto>> GetMatchingRequestsForWalkerAsync(int walkerId, CancellationToken ct = default);

    /// <summary>Walker self-assigns and accepts a Requested walk in one atomic step.</summary>
    Task<WalkEventDto> ClaimWalkAsync(int walkEventId, int walkerId, CancellationToken ct = default);

    /// <summary>Client submits a walk request — creates the event with Requested status.</summary>
    Task<WalkEventDto> ScheduleAsync(CreateWalkEventDto dto, CancellationToken ct = default);

    /// <summary>Admin/system proposes a specific walker to handle the walk request.</summary>
    Task<WalkEventDto> ProposeToWalkerAsync(ProposeWalkDto dto, CancellationToken ct = default);

    /// <summary>Walker accepts or rejects the walk proposal.</summary>
    Task<WalkEventDto> WalkerRespondAsync(WalkerResponseDto dto, CancellationToken ct = default);

    Task<WalkEventDto> UpdateStatusAsync(UpdateWalkStatusDto dto, CancellationToken ct = default);
    Task<WalkEventDto> AssignWalkerAsync(AssignWalkerDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Cancels a walk and optionally records a cancellation note.</summary>
    Task<WalkEventDto> CancelWithNoteAsync(int walkEventId, string? note, CancellationToken ct = default);

    /// <summary>Walker releases an accepted walk, returning it to Requested status.</summary>
    Task<WalkEventDto> UnacceptWalkAsync(int walkEventId, string? note = null, CancellationToken ct = default);

    /// <summary>Returns active walk count vs subscription limit for the given month.</summary>
    Task<MonthlyWalkSummaryDto> GetMonthlySummaryAsync(int clientId, int year, int month, CancellationToken ct = default);
}
