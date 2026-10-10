using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Logistics.Application.Features.DeliverShipment;
using WorldPinball.Logistics.Application.Features.DeliverShipment.Commands;
using WorldPinball.Logistics.Application.Interfaces;
using WorldPinball.Logistics.Domain.Entities;
using WorldPinball.Logistics.Domain.Enums;

namespace WorldPinball.Tests.Logistics.Application;

public class DeliverShipmentHandlerTests
{
    private readonly IShipmentWriteRepository _repository = Substitute.For<IShipmentWriteRepository>();
    private readonly DeliverShipmentHandler _sut;

    public DeliverShipmentHandlerTests() =>
        _sut = new DeliverShipmentHandler(_repository, NullLogger<DeliverShipmentHandler>.Instance);

    [Fact]
    public async Task HandleAsync_WhenValidCommand_ShouldDeliverAndPersist()
    {
        // Arrange
        var shipment = Shipment.Create("ORD-123", "12 Rue de Paris");
        shipment.AllocateMachine(Guid.NewGuid());
        shipment.Dispatch("TRACK-999");
        _repository.GetByIdAsync(shipment.ShipmentId, Arg.Any<CancellationToken>()).Returns(shipment);

        // Act
        var result = await _sut.HandleAsync(new DeliverShipmentCommand(shipment.ShipmentId));

        // Assert
        result.Should().BeOfType<DeliverShipmentResult.Success>();
        shipment.Status.Should().Be(ShipmentStatus.Delivered);
        await _repository.Received(1).UpdateAsync(shipment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenShipmentNotFound_ShouldReturnShipmentNotFound()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        _repository.GetByIdAsync(shipmentId, Arg.Any<CancellationToken>()).Returns((Shipment?)null);

        // Act
        var result = await _sut.HandleAsync(new DeliverShipmentCommand(shipmentId));

        // Assert
        result.Should().BeOfType<DeliverShipmentResult.ShipmentNotFound>()
            .Which.ShipmentId.Should().Be(shipmentId);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Shipment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenShipmentNotDispatched_ShouldReturnCannotDeliver()
    {
        // Arrange
        var shipment = Shipment.Create("ORD-123", "12 Rue de Paris"); // Status is PendingAllocation
        _repository.GetByIdAsync(shipment.ShipmentId, Arg.Any<CancellationToken>()).Returns(shipment);

        // Act
        var result = await _sut.HandleAsync(new DeliverShipmentCommand(shipment.ShipmentId));

        // Assert
        result.Should().BeOfType<DeliverShipmentResult.CannotDeliver>()
            .Which.Reason.Should().Be(Shipment.ErrorMessages.CannotDeliverUnlessInTransit);
        await _repository.DidNotReceive().UpdateAsync(shipment, Arg.Any<CancellationToken>());
    }
}
