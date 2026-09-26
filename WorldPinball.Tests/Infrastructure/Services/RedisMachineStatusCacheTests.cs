using FluentAssertions;
using NSubstitute;
using StackExchange.Redis;
using WorldPinball.Infrastructure.Services;
using WorldPinball.Infrastructure.Services.Redis;

namespace WorldPinball.Tests.Infrastructure.Services;

public class RedisMachineStatusCacheTests
{



    [Fact]
    public async Task GetStatusAsync_WhenKeyExistsInRedis_ShouldReturnStatus()
    {
        // Arrange
        var redisCacheMultiplexer = Substitute.For<IConnectionMultiplexer>();
        
        var db = Substitute.For<IDatabase>();
        var machineId = Guid.NewGuid();
        
        db.StringGetAsync($"machine:status:{machineId}", Arg.Any<CommandFlags>())
            .Returns((RedisValue)"Active");
        
        redisCacheMultiplexer.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(db);
        
        var sut = new RedisMachineStatusCacheService(redisCacheMultiplexer);
        
        //Act
        var result = await sut.GetStatusAsync(machineId);
        
        // Assert
        result.Should().Be("Active");
        
    }

    [Fact]
    public async Task SetStatusAsync_ShouldCallRedisWithCorrectKeyAndTtl()
    {
        var redisCacheMultiplexer = Substitute.For<IConnectionMultiplexer>();
        var db = Substitute.For<IDatabase>();
        redisCacheMultiplexer.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(db);

        var sut = new RedisMachineStatusCacheService(redisCacheMultiplexer);
        var machineId = Guid.NewGuid();
        var ttl = TimeSpan.FromMinutes(5);
        
        
        await sut.SetStatusAsync(machineId, "Active", ttl);
        
        
        await db.Received(1).StringSetAsync(
            $"machine:status:{machineId}",
            "Active",
            ttl);
    }
    
    [Fact]
    public async Task InvalidateStatusAsync_ShouldCallRedisKeyDeleteWithCorrectKey()
    {
        // Arrange
        var redis = Substitute.For<IConnectionMultiplexer>();
        var db = Substitute.For<IDatabase>();
        redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(db);
        var sut = new RedisMachineStatusCacheService(redis);
        var machineId = Guid.NewGuid();
        // Act
        await sut.InvalidateStatusAsync(machineId);
        // Assert
        await db.Received(1).KeyDeleteAsync($"machine:status:{machineId}");
    }
}