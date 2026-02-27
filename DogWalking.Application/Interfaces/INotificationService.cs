using DogWalking.Application.DTOs;

namespace DogWalking.Application.Interfaces;

public interface INotificationService : IDisposable
{
    Task PublishAsync(WalkNotification notification, CancellationToken ct = default);
    event Action<WalkNotification>? NotificationReceived;
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync();
}
