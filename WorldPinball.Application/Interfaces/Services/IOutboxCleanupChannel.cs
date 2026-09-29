using System.Threading.Channels;


namespace WorldPinball.Application.Interfaces.Services;

public interface IOutboxCleanupChannel
{
    ChannelReader<bool> Reader { get; }
    ValueTask RequestCleanupAsync(CancellationToken ct = default);
    
}