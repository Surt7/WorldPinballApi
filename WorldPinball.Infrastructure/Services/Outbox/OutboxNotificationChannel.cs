using System.Threading.Channels;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Infrastructure.Services.Outbox;

/// <summary>
/// High-throughput, lock-free notification channel signaling the Outbox background worker that new events are pending.
/// Uses a bounded <see cref="Channel{T}"/> configured with <see cref="BoundedChannelFullMode.DropWrite"/> to implement
/// wake-up signal coalescing: individual payloads are read in batches from persistence, so dropping redundant signals
/// under extreme burst loads is safe, prevents memory bloat, and provides non-blocking, zero-allocation writes.
/// </summary>
public sealed class OutboxNotificationChannel:IOutboxNotificationChannel
{

    private readonly Channel<bool> _channel;

    public OutboxNotificationChannel(int capacity = 1000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = false,
            SingleWriter = false,
        };
        
        _channel = Channel.CreateBounded<bool>(options);
    }
    public ChannelReader<bool> Reader => _channel.Reader;
    
  
    /// <summary>
    /// Attempts a non-blocking, zero-allocation signal to awaken the outbox processor.
    /// If the channel buffer is full, the signal is safely dropped because the processor is already guaranteed to drain pending batches.
    /// </summary>
    public ValueTask<bool> TryNotifyAsync(CancellationToken ct = default)
    {
        // Non-blocking, zero-allocation write attempt
        return new ValueTask<bool>(_channel.Writer.TryWrite(true));
    }

  
}