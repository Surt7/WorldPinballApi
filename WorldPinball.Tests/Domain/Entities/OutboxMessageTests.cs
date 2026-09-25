using FluentAssertions;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Tests.Domain.Entities;

public class OutboxMessageTests
{
    private static readonly DateTime NowUtc = new(2026, 9, 25, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Constructor_WithValidArguments_ShouldInitializeProperties()
    {
        var messageId = Guid.NewGuid();
        var message = new OutboxMessage(
            id: messageId,
            type: "GameSessionEndedDomainEvent",
            content: "{\"SessionId\":\"...\"}",
            occurredAtUtc: NowUtc
        );

        message.Id.Should().Be(messageId);
        message.Type.Should().Be("GameSessionEndedDomainEvent");
        message.Content.Should().Contain("SessionId");
        message.OccurredAtUtc.Should().Be(NowUtc);
        message.ProcessedAtUtc.Should().BeNull();
        message.Error.Should().BeNull();
        message.RetryCount.Should().Be(0);
    }

    [Fact]
    public void Constructor_WhenIdIsEmpty_ShouldThrowArgumentException()
    {
        Action act = () => new OutboxMessage(Guid.Empty, "Type", "{}", NowUtc);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("id");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WhenTypeIsEmptyOrNull_ShouldThrowArgumentException(string? invalidType)
    {
        Action act = () => new OutboxMessage(Guid.NewGuid(), invalidType!, "{}", NowUtc);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("type");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WhenContentIsEmptyOrNull_ShouldThrowArgumentException(string? invalidContent)
    {
        Action act = () => new OutboxMessage(Guid.NewGuid(), "Type", invalidContent!, NowUtc);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("content");
    }

    [Fact]
    public void MarkAsProcessed_WhenCalled_ShouldSetProcessedAtUtcAndClearError()
    {
        var message = new OutboxMessage(
            Guid.NewGuid(), "GameSessionEndedDomainEvent", "{}", NowUtc);
        var processedAt = NowUtc.AddSeconds(2);

        message.MarkAsProcessed(processedAt);

        message.ProcessedAtUtc.Should().Be(processedAt);
        message.Error.Should().BeNull();
    }

    [Fact]
    public void RecordFailure_WhenCalled_ShouldIncrementRetryCountAndSetError()
    {
        var message = new OutboxMessage(
            Guid.NewGuid(), "GameSessionEndedDomainEvent", "{}", NowUtc);

        message.RecordFailure("Kafka broker connection timeout");

        message.RetryCount.Should().Be(1);
        message.Error.Should().Be("Kafka broker connection timeout");
    }
}