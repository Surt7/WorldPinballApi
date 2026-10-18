using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Logistics.Application.Features.CancelShipment;
using WorldPinball.Logistics.Application.Features.CancelShipment.Commands;
using WorldPinball.Logistics.Application.Interfaces;
using WorldPinball.Logistics.Domain.Entities;
using WorldPinball.Logistics.Domain.Enums;
using WorldPinball.Logistics.Domain.Events;

namespace WorldPinball.Tests.Logistics.Application;

public class CancelShipmentHandlerTests
{
    private readonly IShipmentWriteRepository _repository =
        Substitute.For<IShipmentWriteRepository>();

    private readonly CancelShipmentHandler _sut;

    public CancelShipmentHandlerTests() =>
        _sut = new CancelShipmentHandler(_repository,
            NullLogger<CancelShipmentHandler>.Instance);

    [Fact]
    public async Task HandleAsync_WhenValidCommand_ShouldCancelPersistAndEmitDomainEvent()
    {
        // Arrange
        var shipment = Shipment.Create(
            "ORD-123",
            "12 Rue de Paris");
        _repository.GetByIdAsync(
            shipment.ShipmentId,
            Arg.Any<CancellationToken>()).Returns(shipment);

        var command = new CancelShipmentCommand(
            shipment.ShipmentId,
            "Client cancelled");


        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<CancelShipmentResult.Success>();

        shipment.Status.Should().Be(ShipmentStatus.Cancelled);
        shipment.CancellationReason.Should().Be("Client cancelled");
        
        // Filter linq with only ShipmentCancelledDomainEvent (DomainEvent is a List)
        shipment.DomainEvents.OfType<ShipmentCancelledDomainEvent>()
            .Should().ContainSingle();


        await _repository.Received(1).UpdateAsync(shipment,
            Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task HandleAsync_WhenShipmentNotFound_ShouldReturnShipmentNotFound()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        _repository.GetByIdAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns((Shipment?)null);

        // Act
        var result = await _sut.HandleAsync(new CancelShipmentCommand(
            shipmentId, "Some reason"));


        // Assert
        result.Should().BeOfType<CancelShipmentResult.ShipmentNotFound>()
            .Which.ShipmentId.Should().Be(shipmentId);
        await _repository.DidNotReceive().UpdateAsync(
            Arg.Any<Shipment>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task HandleAsync_WhenReasonIsInvalid_ShouldReturnInvalidCancellationReason(string? invalidReason)
    {
        // Arrange
        var shipmentId = Guid.NewGuid();

        // Act
        var result = await _sut.HandleAsync(new CancelShipmentCommand(shipmentId, invalidReason!));

        // Assert
        result.Should().BeOfType<CancelShipmentResult.InvalidCancellationReason>();
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task HandleAsync_WhenShipmentInTransit_ShouldReturnCannotCancel()
    {
        // Arrange
        var shipment = Shipment.Create("ORD-123", "12 Rue de Paris");
        shipment.AllocateMachine(Guid.NewGuid());
        shipment.Dispatch("TRACK-999");
        _repository.GetByIdAsync(shipment.ShipmentId, Arg.Any<CancellationToken>()).Returns(shipment);

        // Act
        var result = await _sut.HandleAsync(new CancelShipmentCommand(shipment.ShipmentId, "Too late"));

        // Assert
        result.Should().BeOfType<CancelShipmentResult.CannotCancel>()
            .Which.Reason.Should().Be(Shipment.ErrorMessages.CannotCancelDispatchedShipment);
        await _repository.DidNotReceive().UpdateAsync(shipment, Arg.Any<CancellationToken>());
    }
}