using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Logistics.Application.Features.GetShipment;
using WorldPinball.Logistics.Application.Features.GetShipment.Dtos;
using WorldPinball.Logistics.Application.Features.GetShipment.Queries;
using WorldPinball.Logistics.Application.Interfaces;

namespace WorldPinball.Tests.Logistics.Application;

public class GetShipmenByIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenShipmentDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var repository = Substitute.For<IShipmentReadRepository>();
        repository.GetByIdAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns((ShipmentDto?)null);
        var sut = new GetShipmentByIdHandler(repository, NullLogger<GetShipmentByIdHandler>.Instance);
        // Act
        var result = await sut.HandleAsync(new GetShipmentByIdQuery(shipmentId));
        // Assert
        result.Should().BeOfType<GetShipmentByIdResult.NotFound>()
            .Which.ShipmentId.Should().Be(shipmentId);
    }
    
    
    [Fact]
    public async Task HandleAsync_WhenShipmentExists_ShouldReturnSuccessWithShipment()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var expected = new ShipmentDto(shipmentId, "ORD-123", "Paris", "InTransit", Guid.NewGuid(), "TRK-99", null);
        var repository = Substitute.For<IShipmentReadRepository>();
        repository.GetByIdAsync(shipmentId, Arg.Any<CancellationToken>()).Returns(expected);
        var sut = new GetShipmentByIdHandler(repository, NullLogger<GetShipmentByIdHandler>.Instance);

        // Act
        var result = await sut.HandleAsync(new GetShipmentByIdQuery(shipmentId));

        // Assert
        result.Should().BeOfType<GetShipmentByIdResult.Success>()
            .Which.Shipment.Should().BeEquivalentTo(expected);
    }
}