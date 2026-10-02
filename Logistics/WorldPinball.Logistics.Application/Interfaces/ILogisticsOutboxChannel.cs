namespace WorldPinball.Logistics.Application.Interfaces;

using System.Threading.Channels;

public interface ILogisticsOutboxChannel
{
    // wake-up signal
    ValueTask<bool> TryNotifyAsync(CancellationToken ct = default);
    
    
    ChannelReader<bool> Reader { get; }
}