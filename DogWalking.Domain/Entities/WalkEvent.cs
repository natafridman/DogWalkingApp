using DogWalking.Domain.Enums;
using DogWalking.Domain.Exceptions;

namespace DogWalking.Domain.Entities;

/// <summary>
/// Represents a dog walking event from request through completion.
/// Enforces valid status transitions following the full lifecycle:
///   Requested → Proposed → Accepted → InProgress → Completed
/// </summary>
public class WalkEvent
{
    public int       Id                  { get; private set; }
    public int       DogId               { get; private set; }
    public int?      WalkerId            { get; private set; }
    public DateTime  WalkDate            { get; private set; }
    public int       DurationMinutes     { get; private set; }
    public WalkStatus Status             { get; private set; }
    public string    Location            { get; private set; } = string.Empty;
    public DateTime? EstimatedArrivalTime { get; private set; }
    public string?       Notes               { get; private set; }
    public RecurrenceType RecurrenceType   { get; private set; }
    public DateTime  CreatedAt           { get; private set; }
    public byte[]   RowVersion          { get; private set; } = [];

    public Dog   Dog    { get; private set; } = null!;
    public User? Walker { get; private set; }

    private readonly List<WalkDecline> _declines = new();
    public IReadOnlyCollection<WalkDecline> Declines => _declines.AsReadOnly();

    // Required by EF Core
    private WalkEvent() { }

    public WalkEvent(int dogId, DateTime walkDate, int durationMinutes,
                     string location, string? notes = null,
                     RecurrenceType recurrenceType = RecurrenceType.OneTime)
    {
        DogId          = dogId;
        Location       = string.IsNullOrWhiteSpace(location) ? "General" : location.Trim();
        Notes          = notes;
        RecurrenceType = recurrenceType;
        Status         = WalkStatus.Requested;
        CreatedAt      = DateTime.UtcNow;

        SetWalkDate(walkDate);
        SetDuration(durationMinutes);
    }

    /// <summary>
    /// Enforces valid status transitions.
    /// Use the semantic helpers (ProposeToWalker, AcceptByWalker, DeclineByWalker)
    /// for transitions that carry additional data.
    /// </summary>
    public void TransitionTo(WalkStatus newStatus)
    {
        bool isValid = (Status, newStatus) switch
        {
            (WalkStatus.Requested,  WalkStatus.Proposed)   => true,
            (WalkStatus.Requested,  WalkStatus.Cancelled)  => true,
            (WalkStatus.Proposed,   WalkStatus.Accepted)   => true,
            (WalkStatus.Proposed,   WalkStatus.Requested)  => true,  // walker declines — re-queue
            (WalkStatus.Accepted,   WalkStatus.Requested)  => true,  // walker releases an accepted walk
            (WalkStatus.Accepted,   WalkStatus.InProgress) => true,
            (WalkStatus.Accepted,   WalkStatus.Cancelled)  => true,
            (WalkStatus.InProgress, WalkStatus.Completed)  => true,
            (WalkStatus.InProgress, WalkStatus.Cancelled)  => true,
            _ => false
        };

        if (!isValid)
            throw new DomainException(
                $"Invalid walk status transition: '{Status}' → '{newStatus}'.");

        Status = newStatus;
    }

    /// <summary>
    /// Proposes this walk to a specific walker (admin/matching system action).
    /// Sets the walker, estimated arrival time, and transitions to Proposed.
    /// </summary>
    public void ProposeToWalker(int walkerId, DateTime? estimatedArrival = null)
    {
        TransitionTo(WalkStatus.Proposed);
        WalkerId             = walkerId;
        EstimatedArrivalTime = estimatedArrival;
    }

    /// <summary>Walker accepts the proposal — transitions to Accepted.</summary>
    public void AcceptByWalker()  => TransitionTo(WalkStatus.Accepted);

    /// <summary>
    /// Walker releases an already-accepted walk — clears the assignment
    /// and returns the walk to Requested so another walker can claim it.
    /// </summary>
    public void UnacceptByWalker(string? note = null)
    {
        TransitionTo(WalkStatus.Requested);
        WalkerId             = null;
        EstimatedArrivalTime = null;
        if (!string.IsNullOrWhiteSpace(note))
            Notes = note;
    }

    /// <summary>
    /// Walker declines the walk. If the walk was Proposed to this walker,
    /// it transitions back to Requested. If it was already Requested (open request),
    /// the status stays unchanged. In both cases the walker is recorded as having
    /// declined so the walk no longer appears in their schedule.
    /// </summary>
    public void DeclineByWalker(int walkerId)
    {
        if (Status == WalkStatus.Proposed)
        {
            TransitionTo(WalkStatus.Requested);
            WalkerId             = null;
            EstimatedArrivalTime = null;
        }

        if (!_declines.Any(d => d.WalkerId == walkerId))
            _declines.Add(new WalkDecline(walkerId));
    }

    public void UpdateNotes(string? notes) => Notes = notes;

    private void SetWalkDate(DateTime date)
    {
        if (date < DateTime.UtcNow.AddMinutes(-5))
            throw new DomainException("Walk date cannot be in the past.");
        WalkDate = date;
    }

    private void SetDuration(int minutes)
    {
        if (minutes < 15)
            throw new DomainException("Walk duration must be at least 15 minutes.");
        if (minutes > 480)
            throw new DomainException("Walk duration cannot exceed 8 hours.");
        DurationMinutes = minutes;
    }
}
