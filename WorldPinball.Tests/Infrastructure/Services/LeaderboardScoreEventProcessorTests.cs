using System.Text.Json;
using FluentAssertions;
using NSubstitute;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Events;
using WorldPinball.Infrastructure.Services.KafkaConsumers;
using WorldPinball.Infrastructure.Services.Outbox;

namespace WorldPinball.Tests.Infrastructure.Services;

public class LeaderboardScoreEventProcessorTests
{
    private readonly IRealtimeLeaderboardService _leaderboardService =
        Substitute.For<IRealtimeLeaderboardService>();
    
    [Fact]
    public async Task ProcessMessageAsync_WhenScoreSubmittedEvent_ShouldCallAddScoreAsync()
    {
        // Arrange
        var evt = new ScoreSubmittedDomainEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), 1, 
            "USR-1", 1500000, DateTime.UtcNow);
        var json = JsonSerializer.Serialize(evt);
        var sut = new LeaderboardScoreEventProcessor(_leaderboardService);
        // Act
        await sut.ProcessMessageAsync(json, CancellationToken.None);
        // Assert
        await _leaderboardService.Received(1).AddScoreAsync(
            evt.IpId, evt.ModeId, evt.ScoreEntryId, evt.Score, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessMessageAsync_WhenEventHasEmptyScoreEntryId_ShouldNotCallAddScoreAsync()
    {
        var evt = new ScoreSubmittedDomainEvent(
            Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), 1, "USR-1", 1000, DateTime.UtcNow);
        
        var json = JsonSerializer.Serialize(evt);
        var sut = new LeaderboardScoreEventProcessor(_leaderboardService);
        
        
        await sut.ProcessMessageAsync(json, CancellationToken.None);
        
        await _leaderboardService.DidNotReceiveWithAnyArgs()
            .AddScoreAsync(default, default, default, 
                default, Arg.Any<CancellationToken>());
    }



    [Fact]
    public async Task ProcessMessageAsync_WhenCorruptedJson_ShouldNotThrowAndNotCallAddScoreAsync()
    {
        
        // Arrange
        var corruptedJson = "{ not valid json";
        var sut = new LeaderboardScoreEventProcessor(_leaderboardService);
        
        // Act
        var act = () => sut.ProcessMessageAsync(corruptedJson, CancellationToken.None);
        
        // Assert
        await act.Should().NotThrowAsync();
        await _leaderboardService.DidNotReceiveWithAnyArgs()
            .AddScoreAsync(default, default, default, 
                default, Arg.Any<CancellationToken>());
    }




}
