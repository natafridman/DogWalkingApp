using DogWalking.Application.DTOs;
using DogWalking.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DogWalking.Application.Services;

public partial class WalkEventService
{
    public async Task<WalkEventDto> ProposeToWalkerAsync(ProposeWalkDto dto, CancellationToken ct = default)
    {
        var walk = await _uow.WalkEvents.GetByIdAsync(dto.WalkEventId, ct)
            ?? throw new KeyNotFoundException($"Walk event {dto.WalkEventId} not found.");

        var walker = await _uow.Users.GetByIdAsync(dto.WalkerId, ct)
            ?? throw new KeyNotFoundException($"Walker {dto.WalkerId} not found.");

        if (walker.Role != UserRole.Walker)
            throw new InvalidOperationException($"User '{walker.Username}' is not a Walker.");

        walk.ProposeToWalker(dto.WalkerId, dto.EstimatedArrival);
        _uow.WalkEvents.Update(walk);
        await _uow.CommitAsync(ct);
        return MapToDto(walk);
    }

    public async Task<WalkEventDto> UpdateStatusAsync(UpdateWalkStatusDto dto, CancellationToken ct = default)
    {
        var walk = await _uow.WalkEvents.GetByIdAsync(dto.Id, ct)
            ?? throw new KeyNotFoundException($"Walk event {dto.Id} not found.");

        if (dto.NewStatus == WalkStatus.InProgress &&
            walk.WalkDate.ToLocalTime().Date != DateTime.Today)
            throw new InvalidOperationException(
                $"This walk is scheduled for {walk.WalkDate.ToLocalTime():yyyy-MM-dd}. " +
                "You can only start a walk on its scheduled date.");

        walk.TransitionTo(dto.NewStatus);
        _uow.WalkEvents.Update(walk);
        await _uow.CommitAsync(ct);
        _log.LogInformation("Walk event {WalkId} transitioned to {Status}", dto.Id, dto.NewStatus);
        return MapToDto(walk);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var walk = await _uow.WalkEvents.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Walk event {id} not found.");

        if (walk.Status == WalkStatus.InProgress)
            throw new InvalidOperationException("Cannot delete a walk that is in progress.");

        _uow.WalkEvents.Remove(walk);
        await _uow.CommitAsync(ct);
        _log.LogInformation("Walk event {WalkId} deleted", id);
    }

    public async Task<PagedResultDto<WalkEventDto>> GetByStatusPagedAsync(
        WalkStatus status, int page, int pageSize, string? search = null,
        CancellationToken ct = default)
    {
        var (items, total) = await _uow.WalkEvents.GetByStatusPagedAsync(status, page, pageSize, search, ct);
        return new PagedResultDto<WalkEventDto>(
            items.Select(w => MapToDto(w)).ToList(),
            total, page, pageSize);
    }
}
