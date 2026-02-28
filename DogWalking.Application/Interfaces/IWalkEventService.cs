using DogWalking.Application.DTOs;
using DogWalking.Domain.Enums;

namespace DogWalking.Application.Interfaces;

public interface IWalkEventService
{
    Task<IEnumerable<WalkEventDto>> GetByWalkerIdAsync(int walkerId, CancellationToken ct = default);
    Task<IEnumerable<WalkEventDto>> GetByClientIdAsync(int clientId, CancellationToken ct = default);

    Task<IEnumerable<WalkEventDto>> GetMatchingRequestsForWalkerAsync(int walkerId, CancellationToken ct = default);

    Task<WalkEventDto> ClaimWalkAsync(int walkEventId, int walkerId, CancellationToken ct = default);
    Task<WalkEventDto> ScheduleAsync(CreateWalkEventDto dto, CancellationToken ct = default);
    Task<WalkEventDto> ProposeToWalkerAsync(ProposeWalkDto dto, CancellationToken ct = default);
    Task<WalkEventDto> WalkerRespondAsync(WalkerResponseDto dto, CancellationToken ct = default);
    Task<WalkEventDto> UpdateStatusAsync(UpdateWalkStatusDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<WalkEventDto> CancelWithNoteAsync(int walkEventId, string? note, CancellationToken ct = default);
    Task<WalkEventDto> UnacceptWalkAsync(int walkEventId, string? note = null, CancellationToken ct = default);
    Task<MonthlyWalkSummaryDto> GetMonthlySummaryAsync(int clientId, int year, int month, CancellationToken ct = default);
    Task<PagedResultDto<WalkEventDto>> GetByStatusPagedAsync(WalkStatus status, int page, int pageSize, string? search = null, CancellationToken ct = default);
}
