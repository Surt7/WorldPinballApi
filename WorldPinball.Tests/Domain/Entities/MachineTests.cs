
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;
using FluentAssertions;
using NSubstitute.Core;

namespace WorldPinball.Tests.Domain.Entities;




public class MachineTests
{
    [Fact]
    public void Constructor_WhenIpIdIsEmpty_ShouldThrowArgumentException()
    {

        var ipId = Guid.Empty;
        var serialNumber = "WP-K7M4-9X2P-R8HD";
var displayName = "Godzilla — Salle principale";
        Action act = () => new Machine(ipId, serialNumber,displayName);

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
        var machinePinball = new Machine(ipId, serialNumber,displayName);

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
        var machinePinball = new Machine(ipId, serialNumber,displayName);

        // Assert
        machinePinball.MachineId.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WhenSerialNumberIsEmpty_ShouldThrowArgumentException()
    {
        var ipId = Guid.NewGuid();
        var serialNumber = string.Empty;
        var displayName = "Godzilla — Salle principale";
        
        Action act = () => new Machine(ipId, serialNumber,displayName);

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
        
        var machinePinball = new Machine(ipId, serialNumber,displayName);
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
        
        
        var machine = new Machine(ipId, serialNumber,displayName);
            
        
        machine.Status.Should().Be(MachineStatus.PendingEnrollment);
    }
    
    
    [Fact]
    public void Constructor_WhenDisplayNameIsEmpty_ShouldThrowArgumentException()
    {
   
        var ipId = Guid.NewGuid();
        var serialNumber = "WP-K7M4-9X2P-R8HD";
        var displayName = string.Empty;
        
       Action act = () => new Machine(ipId, serialNumber,displayName);
       

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName("displayName");
    }

}
