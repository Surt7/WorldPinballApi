using FluentAssertions;
using NSubstitute;
using StackExchange.Redis;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Infrastructure.Services.Redis;

namespace WorldPinball.Tests.Infrastructure.Services;

public class RedisLiveTelemetryServiceTests
{
    private readonly IConnectionMultiplexer _redis = Substitute.For<IConnectionMultiplexer>();
    private readonly IDatabase _db = Substitute.For<IDatabase>();
    private readonly ILobbyNotificationService _lobbyNotifier = Substitute.For<ILobbyNotificationService>();
    private readonly RedisLiveTelemetryService _sut;

    public RedisLiveTelemetryServiceTests()
    {
        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(_db);
        _sut = new RedisLiveTelemetryService(_redis, _lobbyNotifier);
    }

    [Fact]
    public async Task IngestDeltaAsync_WhenDeltaIsValid_ShouldIncrementRedisKeyAndBroadcastSignalR()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var slotNumber = 1;
        var delta = 15_000L;
        var expectedTotal = 150_000L;
        var redisKey = $"telemetry:session:{sessionId}:slot:{slotNumber}";

        _db.StringIncrementAsync(redisKey, delta).Returns(expectedTotal);

        // Act
        var result = await _sut.IngestDeltaAsync(sessionId, slotNumber, delta, "BUMPER");

        // Assert
        result.Should().Be(expectedTotal);
        await _db.Received(1).KeyExpireAsync(redisKey, TimeSpan.FromHours(2));
        await _lobbyNotifier.Received(1).NotifyScoreSubmittedAsync(sessionId, slotNumber, expectedTotal, Arg.Any<CancellationToken>());
    }
}