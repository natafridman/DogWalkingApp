using DogWalking.Application.DTOs;
using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Services;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DogWalking.Application.Services;

/// <summary>Client operations: schedule, cancel, queries, subscription summary.</summary>
public partial class WalkEventService
{
    public async Task<WalkEventDto> ScheduleAsync(CreateWalkEventDto dto, CancellationToken ct = default)
    {
        await _walkValidator.ValidateAndThrowAsync(dto, ct);

        var dog = await _uow.Dogs.GetByIdWithWalksAsync(dto.DogId, ct)
            ?? throw new KeyNotFoundException($"Dog {dto.DogId} not found.");

        dog.ValidateNoConflictingWalk(dto.WalkDate, dto.DurationMinutes);

        var client = await _uow.Clients.GetByIdAsync(dog.ClientId, ct)
            ?? throw new KeyNotFoundException($"Client for dog {dto.DogId} not found.");

        // Subscription limit check (skipped when admin bypasses)
        if (!dto.BypassSubscriptionLimits)
        {
            var walksThisMonth = await _uow.WalkEvents.GetByClientAndMonthAsync(
                client.Id, dto.WalkDate.Year, dto.WalkDate.Month, ct);

            var strategy = WalkLimitStrategyFactory.Create(client.Subscription);
            strategy.ValidateWalkAllowed(walksThisMonth, dto.WalkDate);
        }

        var dates = GenerateRecurrenceDates(dto.WalkDate, dto.RecurrenceType);

        var walkEvents = dates.Select(date =>
            new WalkEvent(dto.DogId, date, dto.DurationMinutes,
                          dto.Location, dto.Notes, dto.RecurrenceType)).ToList();

        // If the client chose a preferred walker, propose the walk directly
        if (dto.PreferredWalkerId.HasValue)
        {
            var walker = await _uow.Users.GetByIdAsync(dto.PreferredWalkerId.Value, ct)
                ?? throw new KeyNotFoundException($"Walker {dto.PreferredWalkerId.Value} not found.");

            foreach (var walk in walkEvents)
                walk.ProposeToWalker(walker.Id);
        }

        await _uow.WalkEvents.AddRangeAsync(walkEvents, ct);
        await _uow.CommitAsync(ct);

        _log.LogInformation("Walk event(s) scheduled for dog {DogId} on {Date}",
            dto.DogId, dto.WalkDate);

        return MapToDto(walkEvents[0], dog);
    }

    public async Task<WalkEventDto> CancelWithNoteAsync(int walkEventId, string? note,
                                                         CancellationToken ct = default)
    {
        var walk = await _uow.WalkEvents.GetByIdAsync(walkEventId, ct)
            ?? throw new KeyNotFoundException($"Walk event {walkEventId} not found.");

        if (!string.IsNullOrWhiteSpace(note))
            walk.UpdateNotes(note);

        walk.TransitionTo(WalkStatus.Cancelled);
        _uow.WalkEvents.Update(walk);
        await _uow.CommitAsync(ct);
        return MapToDto(walk);
    }

    public async Task<IEnumerable<WalkEventDto>> GetByClientIdAsync(int clientId, CancellationToken ct = default)
    {
        var walks = await _uow.WalkEvents.GetByClientIdAsync(clientId, ct);
        return walks.Select(x => MapToDto(x));
    }

    public async Task<MonthlyWalkSummaryDto> GetMonthlySummaryAsync(
        int clientId, int year, int month, CancellationToken ct = default)
    {
        var client = await _uow.Clients.GetByIdAsync(clientId, ct)
            ?? throw new KeyNotFoundException($"Client {clientId} not found.");

        int active = await _uow.WalkEvents.CountActiveByClientAndMonthAsync(clientId, year, month, ct);
        var strategy = WalkLimitStrategyFactory.Create(client.Subscription);

        int remaining = Math.Max(0, strategy.MaxWalksPerMonth - active);
        return new MonthlyWalkSummaryDto(active, strategy.MaxWalksPerMonth, remaining, strategy.Description);
    }
}
