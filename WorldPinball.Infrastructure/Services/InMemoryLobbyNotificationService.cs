using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using WorldPinball.Application.Interfaces.Services;
using StackExchange.Redis;

namespace WorldPinball.Infrastructure.Services;

public class InMemoryLobbyNotificationService :ILobbyNotificationService
{
    private readonly ISubscriber? _redisSubscriber;
    private static readonly RedisChannel ChannelName = RedisChannel.Literal("worldpinball:lobby:updates");
    private readonly int _capacity;
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Channel<Guid>, byte>> _subscribers = new();
    private readonly ConcurrentDictionary<Channel<Guid>, byte> _globalSubscribers = new();
    
    public InMemoryLobbyNotificationService(IConnectionMultiplexer? redis = null, int capacity = 50)
    {
        _capacity = capacity;
        if (redis is not null)
        {
            _redisSubscriber = redis.GetSubscriber();
            // Whenever an update arrives from Redis, wake up our local subscribers
            _redisSubscriber.Subscribe(ChannelName, (_, message) =>
            {
                if (Guid.TryParse(message.ToString(), out var sessionId))
                {
                    NotifyLocalSubscribers(sessionId);
                }
            });
        }
    }
    
    private void NotifyLocalSubscribers(Guid sessionId)
    {
        if (_subscribers.TryGetValue(sessionId, out var channels))
        {
            foreach (var channel in channels.Keys)
            {
                channel.Writer.TryWrite(sessionId);
            }
        }
        // Notify global admin subscribers
        foreach (var globalChannel in _globalSubscribers.Keys)
        {
            globalChannel.Writer.TryWrite(sessionId);
        }
    }
    
    public void NotifyLobbyUpdated(Guid sessionId)
    {
        // 1. Notify local subscribers on this instance
        NotifyLocalSubscribers(sessionId);
        // 2. Broadcast across all replicas via Redis Pub/Sub
        _redisSubscriber?.PublishAsync(ChannelName, sessionId.ToString());
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

    public Task NotifyMatchFoundAsync(Guid sessionId, Guid machineId, CancellationToken ct = default)
    {
        NotifyLobbyUpdated(sessionId);
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<Guid> SubscribeAllAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {

        var channel = Channel.CreateBounded<Guid>(new BoundedChannelOptions(_capacity)
        {
            SingleWriter = false,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropOldest
        });
        
        _globalSubscribers.TryAdd(channel, 0);
        
        
        try
        {
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
            _globalSubscribers.TryRemove(channel, out _);
        }
    }
    
}