using Confluent.Kafka;
using NSubstitute;
using WorldPinball.Infrastructure.Services;
using WorldPinball.Infrastructure.Services.Outbox;

namespace WorldPinball.Tests.Infrastructure.Services;

public class KafkaEventPublisherTests
{
    [Fact]
    public async Task PublishAsync_WithValidArguments_ShouldCallProduceAsyncOnProducer()
    {
        // Arrange
        var producer = Substitute.For<IProducer<string, string>>();
        var publisher = new KafkaEventPublisher(producer);
        // Act
        await publisher.PublishAsync("game-sessions", "session-123", "{\"score\":1000}");
        // Assert
        await producer.Received(1).ProduceAsync(
            "game-sessions",
            Arg.Is<Message<string, string>>(m => m.Key == "session-123" && m.Value == "{\"score\":1000}"),
            Arg.Any<CancellationToken>());
    }
}