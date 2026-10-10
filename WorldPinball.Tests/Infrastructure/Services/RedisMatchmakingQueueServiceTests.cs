using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using StackExchange.Redis;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Infrastructure.Services.Redis;

namespace WorldPinball.Tests.Infrastructure.Services;

public class RedisMatchmakingQueueServiceTests
{
    private readonly IConnectionMultiplexer _redis = Substitute.For<IConnectionMultiplexer>();
    private readonly IDatabase _db = Substitute.For<IDatabase>();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly RedisMatchmakingQueueService _sut;

    public RedisMatchmakingQueueServiceTests()
    {
        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(_db);
        _sut = new RedisMatchmakingQueueService(_redis, _timeProvider);
    }


    [Fact]
    public async Task EnqueueAsync_WhenCalled_ShouldAddPlayerWithTimestampScore()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 9, 28, 10, 0, 0, TimeSpan.Zero);
        _timeProvider.SetUtcNow(now);
        var playerId = Guid.NewGuid();

        // Act
        await _sut.EnqueueAsync("machine-42", playerId);

        // Assert
        await _db.Received(1).SortedSetAddAsync(
            "matchmaking:queue:machine-42",
            playerId.ToString(),
            now.ToUnixTimeMilliseconds());
    }


    [Fact]
    public async Task DequeueNextAsync_WhenPlayerInQueue_ShouldReturnPlayerId()
    {
        // Arrange
        var expectedPlayerId = Guid.NewGuid();
        var sortedSetEntry =
            new SortedSetEntry(expectedPlayerId.ToString(),
                1000);

        var res = _db.SortedSetPopAsync("matchmaking:queue:machine-42", Order.Ascending)
            .Returns(sortedSetEntry);

        // Act 
        var result = await _sut.DequeueNextAsync("machine-42");

        
        //Assert
        result.Should().Be(expectedPlayerId);
    }

    [Fact]
    public async Task DequeueNextAsync_WhenQueueIsEmpty_ShouldReturnNull()
    {
        _db.SortedSetPopAsync("matchmaking:queue:machine-42", Order.Ascending)
            .Returns((SortedSetEntry?)null);
        
        var result = await _sut.DequeueNextAsync("machine-42");
        
        result.Should().BeNull();
        
    }

    [Fact]
    public async Task RemoveAsync_WhenPlayerInQueue_ShouldReturnTrue()
    {
        
        var playerId = Guid.NewGuid();
        var machineId = "machine-42";
        _db.SortedSetRemoveAsync($"matchmaking:queue:{machineId}", playerId.ToString())
            .Returns(true);
        
        
        var result = await _sut.RemoveAsync(machineId, playerId);
        
        
        result.Should().BeTrue();
        
    }
    [Fact]
    public async Task RemoveAsync_WhenPlayerNotInQueue_ShouldReturnFalse()
    {
        
        var playerId = Guid.NewGuid();
        var machineId = "machine-42";
        _db.SortedSetRemoveAsync($"matchmaking:queue:{machineId}", playerId.ToString())
            .Returns(false);
        
        
        var result = await _sut.RemoveAsync(machineId, playerId);
        
        
        result.Should().BeFalse();
        
    }
    

}