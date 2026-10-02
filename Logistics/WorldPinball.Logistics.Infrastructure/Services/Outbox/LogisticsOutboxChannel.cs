namespace WorldPinball.Logistics.Infrastructure.Services.Outbox;

using System.Threading.Channels;

public sealed class LogisticsOutboxChannel
{

    private readonly Channel<bool> _channel;

    public LogisticsOutboxChannel(int capacity = 1000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropWrite
        };
        
        _channel = Channel.CreateBounded<bool>(options);
    }

    public ChannelReader<bool> Reader => _channel.Reader;

    public ValueTask<bool> TryNotifyAsync(CancellationToken ct = default) =>
        new(_channel.Writer.TryWrite(true));

}