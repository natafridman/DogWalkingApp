namespace DogWalking.Application.DTOs;

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
