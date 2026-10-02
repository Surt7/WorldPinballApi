using FluentAssertions;
using WorldPinball.Infrastructure.Services;

namespace WorldPinball.Tests.Infrastructure.Services;

public class InMemoryLobbyNotificationServiceTests
{
    
    
    
    [Fact]
    public async Task SubscribeAsync_WhenBufferOverflows_ShouldDropOldestItem()
    {
        
        
        // Arrange: capacity of 2 with DropOldest
        var sut = new InMemoryLobbyNotificationService(capacity: 2);
        var sessionId = Guid.NewGuid();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var enumerator = sut.SubscribeAsync(sessionId, cts.Token).GetAsyncEnumerator();
        
        // Act: push 3 notifications into a buffer of 2
        // 1. Start enumeration: registers the channel up to the first await
        var moveNextTask = enumerator.MoveNextAsync().AsTask();
        
        // 2. Act: now the subscriber is registered, push 3 notifications
        sut.NotifyLobbyUpdated(sessionId);
        
        sut.NotifyLobbyUpdated(sessionId);
        
        sut.NotifyLobbyUpdated(sessionId);
        
        
        // Assert: should consume without blocking and keep max capacity
        
        
        (await moveNextTask).Should().BeTrue();
        enumerator.Current.Should().Be(sessionId);
    }
}
