using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Interfaces;
using DogWalking.Domain.Services;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DogWalking.Application.Services;

/// <summary>
/// Orchestrates walk event use cases.
/// Uses the Strategy Pattern (via WalkLimitStrategyFactory) to enforce
/// subscription rules without coupling this service to specific tier logic.
/// </summary>
public class WalkEventService : IWalkEventService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<WalkEventService> _log;
    private readonly IValidator<CreateWalkEventDto> _walkValidator;
    private readonly INotificationService? _notifier;

    public WalkEventService(IUnitOfWork uow, ILogger<WalkEventService> log,
                            IValidator<CreateWalkEventDto> walkValidator,
                            INotificationService? notifier = null)
    {
        _uow = uow;
        _log = log;
        _walkValidator = walkValidator;
        _notifier = notifier;
    }

    public async Task<IEnumerable<WalkEventDto>> GetByDogIdAsync(int dogId, CancellationToken ct = default)
    {
        var walks = await _uow.WalkEvents.GetByDogIdAsync(dogId, ct);
        return walks.Select(x => MapToDto(x));
    }

    public async Task<IEnumerable<WalkEventDto>> GetByStatusAsync(WalkStatus status, CancellationToken ct = default)
    {
        var walks = await _uow.WalkEvents.GetByStatusAsync(status, ct);
        return walks.Select(x => MapToDto(x));
    }

    public async Task<IEnumerable<WalkEventDto>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var walks = await _uow.WalkEvents.GetByDateRangeAsync(from, to, ct);
        return walks.Select(x => MapToDto(x));
    }

    public async Task<IEnumerable<WalkEventDto>> GetByWalkerIdAsync(int walkerId, CancellationToken ct = default)
    {
        var walks = await _uow.WalkEvents.GetByWalkerIdAsync(walkerId, ct);
        return walks.Select(x => MapToDto(x));
    }

    public async Task<IEnumerable<WalkEventDto>> GetByClientIdAsync(int clientId, CancellationToken ct = default)
    {
        var walks = await _uow.WalkEvents.GetByClientIdAsync(clientId, ct);
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
            // Skip walks this walker has already declined
            if (w.Declines.Any(d => d.WalkerId == walkerId))
                return false;

            var local    = w.WalkDate.ToLocalTime();
            var walkTime = TimeOnly.FromDateTime(local);
            var walkDay  = local.DayOfWeek;
            // A slot matches when it covers the day+time AND its zone matches the walk location
            return availability.Any(a => a.DayOfWeek == walkDay
                                      && a.CoversWalk(walkTime, w.DurationMinutes)
                                      && a.IsInZone(w.Location));
        }).Select(w => MapToDto(w));
    }

    public async Task<WalkEventDto> ClaimWalkAsync(int walkEventId, int walkerId,
        CancellationToken ct = default)
    {
        var walk = await _uow.WalkEvents.GetByIdAsync(walkEventId, ct)
            ?? throw new KeyNotFoundException($"Walk event {walkEventId} not found.");

        var walker = await _uow.Users.GetByIdAsync(walkerId, ct)
            ?? throw new KeyNotFoundException($"Walker {walkerId} not found.");

        if (walker.Role != UserRole.Walker)
            throw new InvalidOperationException($"User '{walker.Username}' is not a Walker.");

        // Atomically assign + accept in one domain transaction
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

    public async Task<WalkEventDto> ScheduleAsync(CreateWalkEventDto dto, CancellationToken ct = default)
    {
        await _walkValidator.ValidateAndThrowAsync(dto, ct);

        // Load dog with its walk history for overlap check
        var dog = await _uow.Dogs.GetByIdWithWalksAsync(dto.DogId, ct)
            ?? throw new KeyNotFoundException($"Dog {dto.DogId} not found.");

        // Domain rule: no overlapping active walks for this dog
        dog.ValidateNoConflictingWalk(dto.WalkDate, dto.DurationMinutes);

        // Load the client to know their subscription
        var client = await _uow.Clients.GetByIdAsync(dog.ClientId, ct)
            ?? throw new KeyNotFoundException($"Client for dog {dto.DogId} not found.");

        // Load all walks for ALL client dogs this month (for subscription limit check)
        var walksThisMonth = await _uow.WalkEvents.GetByClientAndMonthAsync(
            client.Id, dto.WalkDate.Year, dto.WalkDate.Month, ct);

        // Strategy Pattern: factory selects the right rule set for this subscription tier
        var strategy = WalkLimitStrategyFactory.Create(client.Subscription);
        strategy.ValidateWalkAllowed(walksThisMonth, dto.WalkDate);

        // Generate all dates (1 for OneTime, multiple for recurring patterns)
        var dates = GenerateRecurrenceDates(dto.WalkDate, dto.RecurrenceType);

        WalkEvent? firstWalk = null;
        foreach (var date in dates)
        {
            var walkEvent = new WalkEvent(dto.DogId, date, dto.DurationMinutes,
                                          dto.Location, dto.Notes, dto.RecurrenceType);
            await _uow.WalkEvents.AddAsync(walkEvent, ct);
            firstWalk ??= walkEvent;
        }

        await _uow.CommitAsync(ct);
        _log.LogInformation("Walk event(s) scheduled for dog {DogId} on {Date}",
            dto.DogId, dto.WalkDate);
        return MapToDto(firstWalk!, dog);
    }

    /// <summary>
    /// Expands a base date into all occurrence dates for the given recurrence pattern.
    /// All arithmetic is done in local time so weekday boundaries are correct.
    /// Dates are returned as UTC.
    /// </summary>
    private static IEnumerable<DateTime> GenerateRecurrenceDates(DateTime baseDate, RecurrenceType recurrence)
    {
        if (recurrence == RecurrenceType.OneTime)
        {
            yield return baseDate;
            yield break;
        }

        var localBase = baseDate.ToLocalTime();
        var timeOfDay = localBase.TimeOfDay;
        var endOfMonth = new DateTime(localBase.Year, localBase.Month,
                                      DateTime.DaysInMonth(localBase.Year, localBase.Month));
        var current = localBase.Date;
        int workingDayIndex = 0;

        while (current <= endOfMonth)
        {
            bool isWorkingDay = current.DayOfWeek is not DayOfWeek.Saturday
                                                  and not DayOfWeek.Sunday;
            bool include = recurrence switch
            {
                RecurrenceType.AllWorkingDays => isWorkingDay,
                RecurrenceType.EveryTwoWorkingDays => isWorkingDay && (workingDayIndex % 2 == 0),
                RecurrenceType.WeeklySameDay => current.DayOfWeek == localBase.DayOfWeek,
                _ => false
            };

            if (isWorkingDay && recurrence == RecurrenceType.EveryTwoWorkingDays)
                workingDayIndex++;

            if (include)
                yield return TimeZoneInfo.ConvertTimeToUtc(current + timeOfDay);

            current = current.AddDays(1);
        }
    }

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

    public async Task<WalkEventDto> UpdateStatusAsync(UpdateWalkStatusDto dto,
                                                       CancellationToken ct = default)
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

    public async Task<WalkEventDto> AssignWalkerAsync(AssignWalkerDto dto,
                                                       CancellationToken ct = default)
    {
        var walk = await _uow.WalkEvents.GetByIdAsync(dto.WalkEventId, ct)
            ?? throw new KeyNotFoundException($"Walk event {dto.WalkEventId} not found.");

        if (dto.WalkerId.HasValue)
        {
            var walker = await _uow.Users.GetByIdAsync(dto.WalkerId.Value, ct)
                ?? throw new KeyNotFoundException($"Walker {dto.WalkerId} not found.");
            if (walker.Role != UserRole.Walker)
                throw new InvalidOperationException($"User '{walker.Username}' is not a Walker.");
            walk.AssignWalker(dto.WalkerId.Value);
        }
        else
        {
            walk.UnassignWalker();
        }

        _uow.WalkEvents.Update(walk);
        await _uow.CommitAsync(ct);
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

    public async Task<MonthlyWalkSummaryDto> GetMonthlySummaryAsync(
        int clientId, int year, int month, CancellationToken ct = default)
    {
        var client = await _uow.Clients.GetByIdAsync(clientId, ct)
            ?? throw new KeyNotFoundException($"Client {clientId} not found.");

        var walks = await _uow.WalkEvents.GetByClientAndMonthAsync(clientId, year, month, ct);
        var strategy = WalkLimitStrategyFactory.Create(client.Subscription);

        int active = walks.Count(w => w.Status is WalkStatus.Requested or WalkStatus.Proposed
                                                or WalkStatus.Accepted  or WalkStatus.InProgress);
        int remaining = Math.Max(0, strategy.MaxWalksPerMonth - active);
        return new MonthlyWalkSummaryDto(active, strategy.MaxWalksPerMonth, remaining, strategy.Description);
    }

    private async Task NotifyAsync(WalkNotification notification, CancellationToken ct)
    {
        if (_notifier is null) return;
        try { await _notifier.PublishAsync(notification, ct); }
        catch { /* Notifications are best-effort */ }
    }

    private static WalkEventDto MapToDto(WalkEvent w, Dog? dog = null) => new(
        w.Id,
        w.DogId,
        dog?.Name ?? w.Dog?.Name ?? string.Empty,
        dog?.Client?.Name ?? w.Dog?.Client?.Name ?? string.Empty,
        w.WalkerId,
        w.Walker?.FullName ?? w.Walker?.Username,
        w.WalkDate,
        w.DurationMinutes,
        w.Status,
        w.Location,
        w.EstimatedArrivalTime,
        w.Notes,
        w.RecurrenceType,
        dog?.Client?.Address ?? w.Dog?.Client?.Address ?? string.Empty
    );
}
