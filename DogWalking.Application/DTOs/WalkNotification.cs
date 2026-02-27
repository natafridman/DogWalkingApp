namespace DogWalking.Application.DTOs;

/// <summary>Categories of walk lifecycle events sent over the notification channel.</summary>
public enum NotificationType
{
    WalkRequested  = 1,
    WalkClaimed    = 2,
    WalkAccepted   = 3,
    WalkDeclined   = 4,
    WalkStarted    = 5,
    WalkCompleted  = 6,
    WalkCancelled  = 7
}

/// <summary>Payload broadcast over UDP when a walk event changes state.</summary>
public record WalkNotification(
    NotificationType Type,
    int    SenderUserId,
    int    WalkEventId,
    int?   ClientId,
    int?   WalkerId,
    string DogName,
    string Location,
    DateTime WalkDate,
    string Message
);
