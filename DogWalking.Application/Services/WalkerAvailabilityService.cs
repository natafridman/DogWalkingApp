using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Interfaces;
using DogWalking.Domain.Services;

namespace DogWalking.Application.Services;

/// <summary>Manages walker availability and walk matching.</summary>
public class WalkerAvailabilityService : IWalkerAvailabilityService
{
    private readonly IUnitOfWork _uow;

    public WalkerAvailabilityService(IUnitOfWork uow) => _uow = uow;

    // ── Availability windows ─────────────────────────────────────────────────

    public async Task<IEnumerable<WalkerAvailabilityDto>> GetByWalkerIdAsync(
        int walkerId, CancellationToken ct = default)
    {
        var slots  = await _uow.WalkerAvailabilities.GetByWalkerIdAsync(walkerId, ct);
        var walker = await _uow.Users.GetByIdAsync(walkerId, ct);
        return slots.Select(s => MapAvailability(s, walker?.FullName ?? string.Empty));
    }

    public async Task<WalkerAvailabilityDto> AddAvailabilityAsync(
        CreateAvailabilityDto dto, CancellationToken ct = default)
    {
        var walker = await _uow.Users.GetByIdAsync(dto.WalkerId, ct)
            ?? throw new KeyNotFoundException($"Walker {dto.WalkerId} not found.");

        if (walker.Role != UserRole.Walker)
            throw new InvalidOperationException($"User '{walker.Username}' is not a Walker.");

        var slot = new WalkerAvailability(dto.WalkerId, dto.DayOfWeek, dto.StartTime, dto.EndTime, dto.Zone);
        await _uow.WalkerAvailabilities.AddAsync(slot, ct);
        await _uow.CommitAsync(ct);
        return MapAvailability(slot, walker.FullName);
    }

    public async Task DeleteAvailabilityAsync(int id, CancellationToken ct = default)
    {
        var slot = await _uow.WalkerAvailabilities.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Availability slot {id} not found.");
        _uow.WalkerAvailabilities.Remove(slot);
        await _uow.CommitAsync(ct);
    }

    // ── Walk matching ────────────────────────────────────────────────────────

    public async Task<IEnumerable<WalkerMatchDto>> FindEligibleWalkersAsync(
        int walkEventId, CancellationToken ct = default)
    {
        var walkEvent = await _uow.WalkEvents.GetByIdAsync(walkEventId, ct)
            ?? throw new KeyNotFoundException($"Walk event {walkEventId} not found.");

        var allAvailabilities = await _uow.WalkerAvailabilities.GetAllAsync(ct);
        var allWalks          = await _uow.WalkEvents.GetByDateRangeAsync(
            walkEvent.WalkDate.Date, walkEvent.WalkDate.Date.AddDays(1), ct);

        var eligibleIds = WalkMatchingService.FindEligibleWalkers(
            walkEvent, allAvailabilities, allWalks).ToHashSet();

        if (eligibleIds.Count == 0) return [];

        var walkers = await _uow.Users.GetAllAsync(ct);
        var zonesByWalker = allAvailabilities
            .Where(a => eligibleIds.Contains(a.WalkerId))
            .GroupBy(a => a.WalkerId)
            .ToDictionary(g => g.Key, g => g.Select(a => a.Zone).Distinct().ToList());

        return walkers
            .Where(u => eligibleIds.Contains(u.Id))
            .Select(u => new WalkerMatchDto(
                u.Id,
                u.FullName,
                zonesByWalker.TryGetValue(u.Id, out var zones) ? zones : []));
    }

    // ── Mappers ──────────────────────────────────────────────────────────────

    private static WalkerAvailabilityDto MapAvailability(WalkerAvailability s, string walkerName) =>
        new(s.Id, s.WalkerId, walkerName, s.DayOfWeek, s.StartTime, s.EndTime, s.Zone);
}
