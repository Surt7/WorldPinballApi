using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using StackExchange.Redis;
using WorldPinball.Api.Filters.Idempotency;

namespace WorldPinball.Tests.Filters;

#pragma warning disable CS0618
public class HybridIdempotencyStoreTests
{
    private readonly InMemoryIdempotencyStore _localStore = new();
    private readonly IConnectionMultiplexer _redis = Substitute.For<IConnectionMultiplexer>();
    private readonly IDatabase _database = Substitute.For<IDatabase>();
    private readonly HybridIdempotencyStore _sut;

    public HybridIdempotencyStoreTests()
    {
        _redis.IsConnected.Returns(true);
        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(_database);
        _sut = new HybridIdempotencyStore(_localStore, NullLogger<HybridIdempotencyStore>.Instance, _redis);
    }

    [Fact]
    public async Task GetAsync_WhenFoundInLocalStore_ShouldReturnImmediatelyWithoutTouchingRedis()
    {
        // Arrange
        var cached = new IdempotentResponse(200, "application/json", "{\"foo\":\"bar\"}");
        await _localStore.SetAsync("key-1", cached, TimeSpan.FromMinutes(10));

        // Act
        var result = await _sut.GetAsync("key-1");

        // Assert
        result.Should().BeEquivalentTo(cached);
        await _database.DidNotReceive().StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>());
    }

    [Fact]
    public async Task GetAsync_WhenMissInLocalStoreAndHitInRedis_ShouldReturnAndWarmUpLocalStore()
    {
        // Arrange
        var redisResponse = new IdempotentResponse(201, "application/json", "{\"id\":123}");
        var json = JsonSerializer.Serialize(redisResponse);
        _database.StringGetAsync((RedisKey)"idempotency:key-2", Arg.Any<CommandFlags>())
            .Returns((RedisValue)json);

        // Act
        var result = await _sut.GetAsync("key-2");

        // Assert
        result.Should().BeEquivalentTo(redisResponse);
        var warmUpCheck = await _localStore.GetAsync("key-2");
        warmUpCheck.Should().BeEquivalentTo(redisResponse);
    }

    [Fact]
    public async Task GetAsync_WhenRedisThrowsException_ShouldCatchGracefullyAndReturnNull()
    {
        // Arrange
        _database.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Throws(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Redis down"));

        // Act
        var result = await _sut.GetAsync("key-down");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WhenCalled_ShouldWriteToBothLocalStoreAndRedis()
    {
        // Arrange
        var response = new IdempotentResponse(200, "application/json", "{\"ok\":true}");

        // Act
        await _sut.SetAsync("key-3", response, TimeSpan.FromMinutes(15));

        // Assert
        var localResult = await _localStore.GetAsync("key-3");
        localResult.Should().BeEquivalentTo(response);
        await _database.Received(1).StringSetAsync(
            (RedisKey)"idempotency:key-3", 
            Arg.Any<RedisValue>(), 
            TimeSpan.FromMinutes(15));
    }

    [Fact]
    public async Task SetAsync_WhenRedisThrowsException_ShouldCatchGracefullyAndLocalStoreStillHasValue()
    {
        // Arrange
        var response = new IdempotentResponse(200, "application/json", "{\"ok\":true}");
        _database.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<Expiration>())
            .Throws(new RedisTimeoutException("Redis timeout", CommandStatus.WaitingToBeSent));

        // Act
        var act = async () => await _sut.SetAsync("key-fail", response, TimeSpan.FromMinutes(15));

        // Assert
        await act.Should().NotThrowAsync();
        var localResult = await _localStore.GetAsync("key-fail");
        localResult.Should().BeEquivalentTo(response);
    }
}
