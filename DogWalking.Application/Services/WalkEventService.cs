using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;
using DogWalking.Domain.Entities;
using DogWalking.Domain.Enums;
using DogWalking.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DogWalking.Application.Services;

public partial class WalkEventService : IWalkEventService
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

    // ── Helpers ──────────────────────────────────────────────

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
}
