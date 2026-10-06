using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Logistics.Application.Features.CreateShipment;
using WorldPinball.Logistics.Application.Features.CreateShipment.Commands;
using WorldPinball.Logistics.Application.Interfaces;
using WorldPinball.Logistics.Domain.Entities;

namespace WorldPinball.Tests.Logistics.Application;

public class CreateShipmentHandlerTests
{
    private readonly IShipmentWriteRepository _repository = Substitute.For<IShipmentWriteRepository>();
    private readonly CreateShipmentHandler _sut;

    public CreateShipmentHandlerTests() =>
        _sut = new CreateShipmentHandler(_repository, NullLogger<CreateShipmentHandler>.Instance);

    [Fact]
    public async Task HandleAsync_WhenValidCommand_ShouldPersistAndReturnSuccess()
    {
        // Arrange
        var command = new CreateShipmentCommand("ORD-12345", "10 Rue de la Paix, Paris");

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<CreateShipmentResult.Success>()
            .Which.ShipmentId.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(Arg.Is<Shipment>(s =>
            s.OrderId == "ORD-12345" && s.DestinationAddress == "10 Rue de la Paix, Paris"));
    }

    [Fact]
    public async Task HandleAsync_WhenShipmentAlreadyExistsForOrder_ShouldReturnShipmentAlreadyExists()
    {
        // Arrange
        var command = new CreateShipmentCommand("ORD-12345", "10 Rue de la Paix, Paris");
        _repository.ExistsByOrderIdAsync("ORD-12345", Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<CreateShipmentResult.ShipmentAlreadyExists>()
            .Which.OrderId.Should().Be("ORD-12345");
        await _repository.DidNotReceive().AddAsync(Arg.Any<Shipment>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task HandleAsync_WhenOrderIdIsInvalid_ShouldReturnInvalidOrderData(string? invalidOrderId)
    {
        // Arrange
        var command = new CreateShipmentCommand(invalidOrderId!, "10 Rue de la Paix, Paris");

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<CreateShipmentResult.InvalidOrderData>()
            .Which.Reason.Should().Be(CreateShipmentHandler.ErrorMessages.EmptyOrderId);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Shipment>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task HandleAsync_WhenAddressIsInvalid_ShouldReturnInvalidOrderData(string? invalidAddress)
    {
        // Arrange
        var command = new CreateShipmentCommand("ORD-12345", invalidAddress!);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<CreateShipmentResult.InvalidOrderData>()
            .Which.Reason.Should().Be(CreateShipmentHandler.ErrorMessages.EmptyAddress);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Shipment>(), Arg.Any<CancellationToken>());
    }
}