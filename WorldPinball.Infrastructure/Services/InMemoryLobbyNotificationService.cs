using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Infrastructure.Services;

public class InMemoryLobbyNotificationService :ILobbyNotificationService
{
    
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Channel<Guid>, byte>> _subscribers = new();
    
    public void NotifyLobbyUpdated(Guid sessionId)
    {
        if (_subscribers.TryGetValue(sessionId, out var channels))
        {
            foreach (var channel in channels.Keys)
            {
                channel.Writer.TryWrite(sessionId);
            }
        }
    }

    // the call attribute [EnumeratorCancellation] create an instance, because of extension 
    public async IAsyncEnumerable<Guid> SubscribeAsync(
        Guid sessionId, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default
        )
    {
        var channel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
        {
            SingleWriter = false,
            SingleReader = true
        });

        var sessionChannels = _subscribers.GetOrAdd(
            sessionId, _ => new ConcurrentDictionary<Channel<Guid>, byte>());
        sessionChannels.TryAdd(channel, 0);
        
        try
        {
            /*
             First, "yield" to ensure iteration goes without blocking whole process
            in second hand we have to rely on machine state (asynchrony principle) 
            */
            while (await channel.Reader.WaitToReadAsync(cancellationToken))
            {
                while (channel.Reader.TryRead(out var id))
                {
                    yield return id;
                }
            }
        }
        finally
        {
            sessionChannels.TryRemove(channel, out _);
            if (sessionChannels.IsEmpty)
            {
                _subscribers.TryRemove(sessionId, out _);
            }
        }
    }
    
    
    public Task NotifyScoreSubmittedAsync(Guid sessionId, int slotNumber, long score, CancellationToken ct = default)
    {
        NotifyLobbyUpdated(sessionId);
        return Task.CompletedTask;
    }
    
    
    public Task NotifySessionLaunchedAsync(Guid sessionId, CancellationToken ct = default)
    {
        NotifyLobbyUpdated(sessionId);
        return Task.CompletedTask;
    }
    public Task NotifySessionEndedAsync(Guid sessionId, CancellationToken ct = default)
    {
        NotifyLobbyUpdated(sessionId);
        return Task.CompletedTask;
    }
    public Task NotifySessionAbortedAsync(Guid sessionId, CancellationToken ct = default)
    {
        NotifyLobbyUpdated(sessionId);
        return Task.CompletedTask;
    }
}