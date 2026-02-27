using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Enums;

namespace DogWalking.Application.Services;

/// <summary>Walker operations: claim, respond, unaccept, schedule queries.</summary>
public partial class WalkEventService
{
    public async Task<WalkEventDto> ClaimWalkAsync(int walkEventId, int walkerId,
        CancellationToken ct = default)
    {
        var walk = await _uow.WalkEvents.GetByIdAsync(walkEventId, ct)
            ?? throw new KeyNotFoundException($"Walk event {walkEventId} not found.");

        var walker = await _uow.Users.GetByIdAsync(walkerId, ct)
            ?? throw new KeyNotFoundException($"Walker {walkerId} not found.");

        if (walker.Role != UserRole.Walker)
            throw new InvalidOperationException($"User '{walker.Username}' is not a Walker.");

        walk.ProposeToWalker(walkerId);
        walk.AcceptByWalker();

        _uow.WalkEvents.Update(walk);
        await _uow.CommitAsync(ct);

        await NotifyAsync(new WalkNotification(
            NotificationType.WalkClaimed, walkerId, walk.Id,
            walk.Dog?.ClientId, walkerId,
            walk.Dog?.Name ?? "", walk.Location, walk.WalkDate,
            $"{walker.FullName} claimed the walk for {walk.Dog?.Name ?? "a dog"}"), ct);

        return MapToDto(walk);
    }

    public async Task<WalkEventDto> WalkerRespondAsync(WalkerResponseDto dto, CancellationToken ct = default)
    {
        var walk = await _uow.WalkEvents.GetByIdAsync(dto.WalkEventId, ct)
            ?? throw new KeyNotFoundException($"Walk event {dto.WalkEventId} not found.");

        if (dto.Accepted)
        {
            walk.AcceptByWalker();
        }
        else
        {
            walk.DeclineByWalker(dto.WalkerId);
            if (!string.IsNullOrWhiteSpace(dto.DeclineNote))
                walk.UpdateNotes(dto.DeclineNote);
        }

        _uow.WalkEvents.Update(walk);
        await _uow.CommitAsync(ct);

        if (dto.Accepted)
        {
            var dogName = walk.Dog?.Name ?? "";
            await NotifyAsync(new WalkNotification(
                NotificationType.WalkAccepted, dto.WalkerId, walk.Id,
                walk.Dog?.ClientId, dto.WalkerId,
                dogName, walk.Location, walk.WalkDate,
                $"Walk for {dogName} was accepted"), ct);
        }

        return MapToDto(walk);
    }

    public async Task<WalkEventDto> UnacceptWalkAsync(int walkEventId, string? note = null,
                                                       CancellationToken ct = default)
    {
        var walk = await _uow.WalkEvents.GetByIdAsync(walkEventId, ct)
            ?? throw new KeyNotFoundException($"Walk event {walkEventId} not found.");

        walk.UnacceptByWalker(note);
        _uow.WalkEvents.Update(walk);
        await _uow.CommitAsync(ct);
        return MapToDto(walk);
    }

    public async Task<IEnumerable<WalkEventDto>> GetByWalkerIdAsync(int walkerId, CancellationToken ct = default)
    {
        var walks = await _uow.WalkEvents.GetByWalkerIdAsync(walkerId, ct);
        return walks.Select(x => MapToDto(x));
    }

    public async Task<IEnumerable<WalkEventDto>> GetProposedForWalkerAsync(int walkerId, CancellationToken ct = default)
    {
        var walks = await _uow.WalkEvents.GetByWalkerIdAsync(walkerId, ct);
        return walks.Where(w => w.Status == WalkStatus.Proposed).Select(x => MapToDto(x));
    }

    public async Task<IEnumerable<WalkEventDto>> GetMatchingRequestsForWalkerAsync(
        int walkerId, CancellationToken ct = default)
    {
        var requested    = await _uow.WalkEvents.GetByStatusAsync(WalkStatus.Requested, ct);
        var availability = await _uow.WalkerAvailabilities.GetByWalkerIdAsync(walkerId, ct);

        return requested.Where(w =>
        {
            if (w.Declines.Any(d => d.WalkerId == walkerId))
                return false;

            var local    = w.WalkDate.ToLocalTime();
            var walkTime = TimeOnly.FromDateTime(local);
            var walkDay  = local.DayOfWeek;
            return availability.Any(a => a.DayOfWeek == walkDay
                                      && a.CoversWalk(walkTime, w.DurationMinutes)
                                      && a.IsInZone(w.Location));
        }).Select(w => MapToDto(w));
    }
}
