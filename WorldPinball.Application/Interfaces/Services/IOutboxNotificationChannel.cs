using System.Threading.Channels;

namespace WorldPinball.Application.Interfaces.Services;

public interface IOutboxNotificationChannel
{
    // Emits a wake-up signal to background processors
    ValueTask<bool> TryNotifyAsync(CancellationToken ct = default);

    // Stream reader consumed by the background service
    ChannelReader<bool> Reader { get; }
}
