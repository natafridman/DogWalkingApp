using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DogWalking.Application.DTOs;
using DogWalking.Application.Interfaces;

namespace DogWalking.Infrastructure.Messaging;

/// <summary>
/// LAN notification service using UDP multicast.
/// All app instances on the same network automatically receive broadcasts.
/// Uses a long-lived sender UdpClient to avoid socket allocation per publish.
/// Swap this implementation for cloud messaging (Azure Service Bus, RabbitMQ, etc.)
/// by registering a different INotificationService in DI.
/// </summary>
public class UdpNotificationService : INotificationService
{
    private UdpClient? _listener;
    private UdpClient? _sender;
    private CancellationTokenSource? _cts;
    private static readonly IPAddress MulticastAddress = IPAddress.Parse("239.255.0.1");
    private const int Port = 5150;

    public event Action<WalkNotification>? NotificationReceived;

    public async Task PublishAsync(WalkNotification notification, CancellationToken ct = default)
    {
        try
        {
            _sender ??= new UdpClient();
            var json  = JsonSerializer.Serialize(notification);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _sender.SendAsync(bytes, new IPEndPoint(MulticastAddress, Port), ct);
        }
        catch (SocketException) { /* Network unavailable — best-effort */ }
        catch (ObjectDisposedException) { _sender = null; }
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        _listener = new UdpClient();
        _listener.Client.SetSocketOption(
            SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _listener.Client.Bind(new IPEndPoint(IPAddress.Any, Port));
        _listener.JoinMulticastGroup(MulticastAddress);

        _ = ListenLoopAsync(_cts.Token);
        return Task.CompletedTask;
    }

    private async Task ListenLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var result = await _listener!.ReceiveAsync(ct);
                var json   = Encoding.UTF8.GetString(result.Buffer);
                var notification = JsonSerializer.Deserialize<WalkNotification>(json);
                if (notification is not null)
                    NotificationReceived?.Invoke(notification);
            }
        }
        catch (OperationCanceledException) { }
        catch (SocketException) { }
        catch (ObjectDisposedException) { }
    }

    public Task StopAsync()
    {
        _cts?.Cancel();
        try { _listener?.DropMulticastGroup(MulticastAddress); } catch { }
        _listener?.Dispose();
        _listener = null;
        _sender?.Dispose();
        _sender = null;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
