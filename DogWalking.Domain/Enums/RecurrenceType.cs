namespace DogWalking.Domain.Enums;

/// <summary>
/// Describes how a walk request repeats within the rest of the current month.
/// </summary>
public enum RecurrenceType
{
    OneTime             = 0,  // Single walk — existing default behaviour
    AllWorkingDays      = 1,  // Every Mon–Fri from base date to end of month
    EveryTwoWorkingDays = 2,  // Every other Mon–Fri from base date to end of month
    WeeklySameDay       = 3   // Same weekday every week until end of month
}
