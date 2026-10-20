using FluentAssertions;
using WorldPinball.Infrastructure.Services.Outbox;
using Xunit;
namespace WorldPinball.Tests.Infrastructure.Services;
public class OutboxNotificationChannelTests
{
    [Fact]
    public async Task TryNotifyAsync_WhenChannelHasCapacity_ShouldReturnTrueAndAllowRead()
    {
        var channel = new OutboxNotificationChannel(capacity: 2);
     
        var result = await channel.TryNotifyAsync();

        result.Should().BeTrue();
        channel.Reader.TryRead(out var signal).Should().BeTrue();
    }
    
    [Fact]
    public async Task TryNotifyAsync_WhenChannelIsFull_ShouldDropSignalAndReturnFalseWithoutThrowing()
    {
        // Arrange: channel with capacity 1
        var channel = new OutboxNotificationChannel(capacity: 1);
        // Act: fill the channel, then attempt a second write
        await channel.TryNotifyAsync();
        await channel.TryNotifyAsync();
        // Assert: only 1 item exists in queue, the second was dropped
        channel.Reader.TryRead(out _).Should().BeTrue();
        channel.Reader.TryRead(out _).Should().BeFalse("Queue should only contain 1 item since capacity is 1");
    }
    
}