using System.Threading.Channels;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Infrastructure.Services.Outbox;

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
    
  
    public ValueTask<bool> TryNotifyAsync(CancellationToken ct = default)
    {
        // Non-blocking, zero-allocation write attempt
        return new ValueTask<bool>(_channel.Writer.TryWrite(true));
    }

  
}