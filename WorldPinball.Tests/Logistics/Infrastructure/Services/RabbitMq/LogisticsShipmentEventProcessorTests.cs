using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WorldPinball.Logistics.Domain.Events;
using WorldPinball.Logistics.Infrastructure.Services.RabbitMq;
using Xunit;

namespace WorldPinball.Tests.Logistics.Infrastructure.Services.RabbitMq;

public class LogisticsShipmentEventProcessorTests
{
    private readonly ILogger<LogisticsShipmentEventProcessor> _logger =
        Substitute.For<ILogger<LogisticsShipmentEventProcessor>>();
    private readonly LogisticsShipmentEventProcessor _sut;
    
    public LogisticsShipmentEventProcessorTests()
    {
        _sut = new LogisticsShipmentEventProcessor(_logger);
    }
    
    [Fact]
    public async Task ProcessMessageAsync_WhenShipmentCreated_ShouldProcessSuccessfully()
    {
        // Arrange
        var evt = new ShipmentCreatedDomainEvent(
            Guid.NewGuid(),
            "ORD-999",
            "12 Rue de la Paix, Paris"
        );
        var json = JsonSerializer.Serialize(evt);
        // Act
        var act = () => _sut.ProcessMessageAsync(nameof(ShipmentCreatedDomainEvent), json);
        // Assert
        await act.Should().NotThrowAsync();
    }
}