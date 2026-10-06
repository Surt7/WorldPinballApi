using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Logistics.Application.Features.AllocateMachine;
using WorldPinball.Logistics.Application.Features.AllocateMachine.Commands;
using WorldPinball.Logistics.Application.Interfaces;
using WorldPinball.Logistics.Domain.Entities;
using WorldPinball.Logistics.Domain.Enums;

namespace WorldPinball.Tests.Logistics.Application;

public class AllocateMachineHandlerTests
{
    private readonly IShipmentWriteRepository _repository = Substitute.For<IShipmentWriteRepository>();
    private readonly AllocateMachineHandler _sut;

    public AllocateMachineHandlerTests() =>
        _sut = new AllocateMachineHandler(_repository, NullLogger<AllocateMachineHandler>.Instance);

    [Fact]
    public async Task HandleAsync_WhenValidCommand_ShouldAllocateMachineAndPersist()
    {
        // Arrange
        var shipment = Shipment.Create("ORD-12345", "10 Rue de la Paix, Paris");
        var machineId = Guid.NewGuid();
        _repository.GetByIdAsync(shipment.ShipmentId, Arg.Any<CancellationToken>()).Returns(shipment);

        // Act
        var result = await _sut.HandleAsync(new AllocateMachineCommand(shipment.ShipmentId, machineId));

        // Assert
        result.Should().BeOfType<AllocateMachineResult.Success>();
        shipment.Status.Should().Be(ShipmentStatus.MachineAllocated);
        shipment.MachineId.Should().Be(machineId);
        await _repository.Received(1).UpdateAsync(shipment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenShipmentNotFound_ShouldReturnShipmentNotFound()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        _repository.GetByIdAsync(shipmentId, Arg.Any<CancellationToken>()).Returns((Shipment?)null);

        // Act
        var result = await _sut.HandleAsync(new AllocateMachineCommand(shipmentId, Guid.NewGuid()));

        // Assert
        result.Should().BeOfType<AllocateMachineResult.ShipmentNotFound>()
            .Which.ShipmentId.Should().Be(shipmentId);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Shipment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenMachineIdIsEmpty_ShouldReturnInvalidMachineId()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();

        // Act
        var result = await _sut.HandleAsync(new AllocateMachineCommand(shipmentId, Guid.Empty));

        // Assert
        result.Should().BeOfType<AllocateMachineResult.InvalidMachineId>();
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenShipmentAlreadyAllocated_ShouldReturnCannotAllocate()
    {
        // Arrange
        var shipment = Shipment.Create("ORD-12345", "10 Rue de la Paix, Paris");
        shipment.AllocateMachine(Guid.NewGuid()); // Already allocated!
        _repository.GetByIdAsync(shipment.ShipmentId, Arg.Any<CancellationToken>()).Returns(shipment);

        // Act
        var result = await _sut.HandleAsync(new AllocateMachineCommand(shipment.ShipmentId, Guid.NewGuid()));

        // Assert
        result.Should().BeOfType<AllocateMachineResult.CannotAllocate>()
            .Which.Reason.Should().Be(Shipment.ErrorMessages.CannotAllocateUnlessPending);
        await _repository.DidNotReceive().UpdateAsync(shipment, Arg.Any<CancellationToken>());
    }
}