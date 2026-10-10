using FluentAssertions;
using WorldPinball.Infrastructure.Security.Authorization;
using Xunit;

namespace WorldPinball.Tests.Infrastructure.Security.Authorization;

public class RolePermissionStoreTests
{
    [Fact]
    public void HasPermission_WhenRoleHasPermission_ShouldReturnTrue()
    {
        // Arrange
        var data = new Dictionary<string, IEnumerable<string>>
        {
            ["Technician"] = ["MachinesProvision", "ShipmentsDispatch"]
        };
        var sut = new FrozenRolePermissionStore(data);

        // Act
        var result = sut.HasPermission("Technician", "MachinesProvision");

        // Assert
        result.Should().BeTrue();
    }
    
    
    [Fact]
    public void HasPermission_WhenCaseDiffers_ShouldReturnTrue()
    {
        // Arrange
        var data = new Dictionary<string, IEnumerable<string>>
        {
            ["Technician"] = ["MachinesProvision"]
        };
        var sut = new FrozenRolePermissionStore(data);

        // Act
        var result = sut.HasPermission("technician", "machinesprovision");

        // Assert
        result.Should().BeTrue();
    }
    
    
    [Fact]
    public void HasPermission_WhenRoleOrPermissionMissing_ShouldReturnFalse()
    {
        // Arrange
        var data = new Dictionary<string, IEnumerable<string>>
        {
            ["Technician"] = ["MachinesProvision"]
        };
        var sut = new FrozenRolePermissionStore(data);

        // Act & Assert
        sut.HasPermission("Technician", "ShipmentsDispatch").Should().BeFalse();
        sut.HasPermission("UnknownRole", "MachinesProvision").Should().BeFalse();
    }
    
    
    [Fact]
    public void HasPermission_WhenOneOfMultipleRolesHasPermission_ShouldReturnTrue()
    {
        // Arrange
        var data = new Dictionary<string, IEnumerable<string>>
        {
            ["Player"] = ["SessionsPlay"],
            ["Technician"] = ["MachinesProvision"]
        };
        var sut = new FrozenRolePermissionStore(data);
        // Act
        var result = sut.HasPermission(["Player", "Technician"], "MachinesProvision");
        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Reload_WhenPermissionsUpdated_ShouldReflectNewPermissionsAtomically()
    {
        // Arrange
        var sut = new DynamicRolePermissionStore(new Dictionary<string, IEnumerable<string>>
        {
            ["Technician"] = ["MachinesProvision"]
        });

        sut.HasPermission("Technician", "ShipmentsDispatch").Should().BeFalse();

        // Act
        sut.Reload(new Dictionary<string, IEnumerable<string>>
        {
            ["Technician"] = ["MachinesProvision", "ShipmentsDispatch"]
        });

        // Assert
        sut.HasPermission("Technician", "ShipmentsDispatch").Should().BeTrue();
    }
}