using System.Threading.Channels;
using System.Threading.Tasks;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Infrastructure.Services.Outbox;

public class OutboxCleanupChannel:IOutboxCleanupChannel
{
    private readonly Channel<bool> _cleanupChannel = Channel.CreateBounded<bool>(
        new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

    public ChannelReader<bool> Reader => _cleanupChannel.Reader;
    
    public ValueTask RequestCleanupAsync(CancellationToken ct = default)
    {
        return _cleanupChannel.Writer.WriteAsync(true, ct);
    }
}