using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using WorldPinball.Logistics.Infrastructure.OptionConfigurations;
using WorldPinball.Logistics.Infrastructure.Services.RabbitMq;
using Xunit;



namespace WorldPinball.Tests.Infrastructure.Services.RabbitMq;

public class RabbitMqLogisticsEventPublisherTests
{
    private readonly RabbitMqLogisticsEventPublisher _sut;
    
    public RabbitMqLogisticsEventPublisherTests()
    {
        var options = Options.Create(new RabbitMqOptions
        {
            HostName = "localhost",
            Port = 5672,
            ExchangeName = "logistics.events"
        });
        var logger = Substitute.For<ILogger<RabbitMqLogisticsEventPublisher>>();
        _sut = new RabbitMqLogisticsEventPublisher(options, logger);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task PublishAsync_WhenTopicIsNullOrEmpty_ShouldThrowArgumentException(string? invalidTopic)
    {
        // Act
        var act = () => _sut.PublishAsync(invalidTopic!, "key-1", "{}");
        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
    
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task PublishAsync_WhenKeyIsNullOrEmpty_ShouldThrowArgumentException(string? invalidKey)
    {
        // Act
        var act = () => _sut.PublishAsync("topic", invalidKey!, "{}");
        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
    
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task PublishAsync_WhenMessageIsNullOrEmpty_ShouldThrowArgumentException(string? invalidMessage)
    {
        // Act
        var act = () => _sut.PublishAsync("topic", "key-1", invalidMessage!);
        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DisposeAsync_WhenNeverConnected_ShouldCompleteWithoutThrowing()
    {
        // Act
        var act = () => _sut.DisposeAsync().AsTask();
        // Assert
        await act.Should().NotThrowAsync();
    }
    [Fact]
    public async Task PublishAsync_WhenAlreadyDisposed_ShouldThrowObjectDisposedException()
    {
        // Arrange
        await _sut.DisposeAsync();

        // Act
        var act = () => _sut.PublishAsync("topic", "key-1", "{}");

        // Assert
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }
    
}