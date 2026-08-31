
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;
using FluentAssertions;
namespace WorldPinball.Tests.Domain.Entities;




public class MachineTests
{
    [Fact]
    public void Constructor_WhenIpIdIsEmpty_ShouldThrowArgumentException()
    {

        var ipId = Guid.Empty;
        var serialNumber = "WP-K7M4-9X2P-R8HD";

        Action act = () => new Machine(ipId, serialNumber);

        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName("ipId");
    }


    [Fact]
    public void Constructor_WithValidIpId_ShouldAssignIpId()
    {
        // Arrange
        var ipId = Guid.NewGuid();
        var serialNumber = "WP-K7M4-9X2P-R8HD";
        // Act
        var machinePinball = new Machine(ipId, serialNumber);

        // Assert
        machinePinball.IpId.Should()
            .Be(ipId);
    }

    [Fact]
    public void Constructor_ShouldGenerateMachineId()
    {
        // Arrange
        var ipId = Guid.NewGuid();
        var serialNumber = "WP-K7M4-9X2P-R8HD";
        // Act
        var machinePinball = new Machine(ipId, serialNumber);

        // Assert
        machinePinball.MachineId.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WhenSerialNumberIsEmpty_ShouldThrowArgumentException()
    {
        var ipId = Guid.NewGuid();
        var serialNumber = string.Empty;

        Action act = () => new Machine(ipId, serialNumber);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("serialNumber");

    }

    [Fact]
    public void Activate_WhenMachineIsRevoked_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ipId = Guid.NewGuid();
        var serialNumber = "WP-K7M4-9X2P-R8HD";
        var machinePinball = new Machine(ipId, serialNumber);
        machinePinball.SetStatus(MachineStatus.Revoked);

        // Act
        Action act = machinePinball.Activate;

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("A revoked machine cannot be reactivated.");
    }

    [Fact]
    public void Constructor_ShouldInitializeStatusToPendingEnrollment()
    {
        // Arrange & Act
        var machine = new Machine(
            Guid.NewGuid(),
            "WP-K7M4-9X2P-R8HD");

        // Assert
        machine.Status.Should().Be(MachineStatus.PendingEnrollment);
    }

}
