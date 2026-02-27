namespace DogWalking.Domain.Enums;

/// <summary>
/// Lifecycle states of a WalkEvent.
/// Valid transitions:
///   Requested  → Proposed  | Cancelled
///   Proposed   → Accepted  | Requested (walker declines — re-queued)
///   Accepted   → InProgress | Cancelled | Requested (walker releases)
///   InProgress → Completed | Cancelled
/// </summary>
public enum WalkStatus
{
    Requested  = 1,  // Owner submitted a walk request
    Proposed   = 2,  // System/Admin proposed to an eligible walker
    Accepted   = 3,  // Walker accepted the walk
    InProgress = 5,  // Walk is underway
    Completed  = 6,  // Walk finished successfully
    Cancelled  = 7   // Cancelled by owner or admin
}
