using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Logistics.Application.Features.DispatchShipment;
using WorldPinball.Logistics.Application.Features.DispatchShipment.Commands;
using WorldPinball.Logistics.Application.Interfaces;
using WorldPinball.Logistics.Domain.Entities;
using WorldPinball.Logistics.Domain.Enums;

namespace WorldPinball.Tests.Logistics.Application;

public class DispatchShipmentHandlerTests
{
    private readonly IShipmentWriteRepository _repository = Substitute.For<IShipmentWriteRepository>();
    private readonly DispatchShipmentHandler _sut;

    public DispatchShipmentHandlerTests() =>
        _sut = new DispatchShipmentHandler(_repository, NullLogger<DispatchShipmentHandler>.Instance);

    [Fact]
    public async Task HandleAsync_WhenValidCommand_ShouldDispatchAndPersist()
    {
        // Arrange
        var shipment = Shipment.Create("ORD-123", "12 Rue de Paris");
        shipment.AllocateMachine(Guid.NewGuid());
        _repository.GetByIdAsync(shipment.ShipmentId, Arg.Any<CancellationToken>()).Returns(shipment);

        // Act
        var result = await _sut.HandleAsync(new DispatchShipmentCommand(shipment.ShipmentId, "TRACK-999"));

        // Assert
        result.Should().BeOfType<DispatchShipmentResult.Success>();
        shipment.Status.Should().Be(ShipmentStatus.InTransit);
        shipment.TrackingNumber.Should().Be("TRACK-999");
        await _repository.Received(1).UpdateAsync(shipment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenShipmentNotFound_ShouldReturnShipmentNotFound()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        _repository.GetByIdAsync(shipmentId, Arg.Any<CancellationToken>()).Returns((Shipment?)null);

        // Act
        var result = await _sut.HandleAsync(new DispatchShipmentCommand(shipmentId, "TRACK-999"));

        // Assert
        result.Should().BeOfType<DispatchShipmentResult.ShipmentNotFound>()
            .Which.ShipmentId.Should().Be(shipmentId);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Shipment>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task HandleAsync_WhenTrackingNumberIsInvalid_ShouldReturnInvalidTrackingNumber(string? invalidTracking)
    {
        // Arrange
        var shipmentId = Guid.NewGuid();

        // Act
        var result = await _sut.HandleAsync(new DispatchShipmentCommand(shipmentId, invalidTracking!));

        // Assert
        result.Should().BeOfType<DispatchShipmentResult.InvalidTrackingNumber>();
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenShipmentNotAllocated_ShouldReturnCannotDispatch()
    {
        // Arrange
        var shipment = Shipment.Create("ORD-123", "12 Rue de Paris"); // Status is PendingAllocation
        _repository.GetByIdAsync(shipment.ShipmentId, Arg.Any<CancellationToken>()).Returns(shipment);

        // Act
        var result = await _sut.HandleAsync(new DispatchShipmentCommand(shipment.ShipmentId, "TRACK-999"));

        // Assert
        result.Should().BeOfType<DispatchShipmentResult.CannotDispatch>()
            .Which.Reason.Should().Be(Shipment.ErrorMessages.CannotDispatchUnlessAllocated);
        await _repository.DidNotReceive().UpdateAsync(shipment, Arg.Any<CancellationToken>());
    }
}