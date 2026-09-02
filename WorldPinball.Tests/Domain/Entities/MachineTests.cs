
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;
using FluentAssertions;

namespace WorldPinball.Tests.Domain.Entities;

public class MachineTests
{
    private static readonly DateTime RegisteredAtUtc = new(
        2026, 9, 2, 12, 0, 0,
        DateTimeKind.Utc);

    [Fact]
    public void Constructor_WhenIpIdIsEmpty_ShouldThrowArgumentException()
    {
        var ipId = Guid.Empty;
        var serialNumber = "WP-K7M4-9X2P-R8HD";
        var displayName = "Godzilla — Salle principale";

        Action act = () => new Machine(
            ipId,
            serialNumber,
            displayName,
            RegisteredAtUtc);

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
        var displayName = "Godzilla — Salle principale";

        // Act
        var machinePinball = new Machine(
            ipId,
            serialNumber,
            displayName,
            RegisteredAtUtc);

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
        var displayName = "Godzilla — Salle principale";

        // Act
        var machinePinball = new Machine(
            ipId,
            serialNumber,
            displayName,
            RegisteredAtUtc);

        // Assert
        machinePinball.MachineId.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_ShouldAssignRegisteredAtUtc()
    {
        // Arrange
        var ipId = Guid.NewGuid();

        // Act
        var machine = new Machine(
            ipId,
            "WP-K7M4-9X2P-R8HD",
            "Godzilla — Salle principale",
            RegisteredAtUtc);

        // Assert
        machine.RegisteredAtUtc.Should().Be(RegisteredAtUtc);
    }

    [Fact]
    public void Constructor_WhenSerialNumberIsEmpty_ShouldThrowArgumentException()
    {
        var ipId = Guid.NewGuid();
        var serialNumber = string.Empty;
        var displayName = "Godzilla — Salle principale";

        Action act = () => new Machine(
            ipId,
            serialNumber,
            displayName,
            RegisteredAtUtc);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("serialNumber");

    }

    [Fact]
    public void Activate_WhenMachineIsRevoked_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ipId = Guid.NewGuid();
        var serialNumber = "WP-K7M4-9X2P-R8HD";
        var displayName = "Godzilla — Salle principale";
        
        var machinePinball = new Machine(
            ipId,
            serialNumber,
            displayName,
            RegisteredAtUtc);
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
        var ipId = Guid.NewGuid();
        var serialNumber = "WP-K7M4-9X2P-R8HD";
        var displayName = "Godzilla — Salle principale";

        var machine = new Machine(
            ipId,
            serialNumber,
            displayName,
            RegisteredAtUtc);

        machine.Status.Should().Be(MachineStatus.PendingEnrollment);
    }
    
    
    [Fact]
    public void Constructor_WhenDisplayNameIsEmpty_ShouldThrowArgumentException()
    {
        var ipId = Guid.NewGuid();
        var serialNumber = "WP-K7M4-9X2P-R8HD";
        var displayName = string.Empty;

        Action act = () => new Machine(
            ipId,
            serialNumber,
            displayName,
            RegisteredAtUtc);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName("displayName");
    }

}
