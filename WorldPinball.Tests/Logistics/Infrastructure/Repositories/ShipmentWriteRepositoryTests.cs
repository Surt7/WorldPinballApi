using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WorldPinball.Logistics.Domain.Entities;
using WorldPinball.Logistics.Domain.Enums;
using WorldPinball.Logistics.Infrastructure;
using WorldPinball.Logistics.Infrastructure.Persistence;
using WorldPinball.Logistics.Infrastructure.Persistence.Repositories;

namespace WorldPinball.Tests.Logistics.Infrastructure.Repositories;

public class ShipmentWriteRepositoryTests
{
    private readonly LogisticsDbContext _dbContext;
    private readonly ShipmentWriteRepository _sut;
    
    public ShipmentWriteRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<LogisticsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new LogisticsDbContext(options);
        _sut = new ShipmentWriteRepository(_dbContext);
    }
    
    [Fact]
    public async Task AddAsync_WhenValidShipment_ShouldPersistInDatabase()
    {
        // Arrange
        var shipment = Shipment.Create("ORDER-123", "123 Main St");
        // Act
        await _sut.AddAsync(shipment);
        // Assert
        var saved = await _dbContext.Shipments.FirstOrDefaultAsync(s => s.ShipmentId == shipment.ShipmentId);
        saved.Should().NotBeNull();
        saved!.OrderId.Should().Be("ORDER-123");
    }
    
    
    [Fact]
    public async Task GetByIdAsync_WhenShipmentExists_ShouldReturnShipment()
    {
        // Arrange
        var shipment = Shipment.Create("ORDER-456", "456 Oak Avenue");
        await _sut.AddAsync(shipment);

        // Act
        var result = await _sut.GetByIdAsync(shipment.ShipmentId);

        // Assert
        result.Should().NotBeNull();
        result!.OrderId.Should().Be("ORDER-456");
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenShipmentDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByOrderIdAsync_WhenOrderExists_ShouldReturnTrue()
    {
        // Arrange
        var shipment = Shipment.Create("ORDER-789", "789 Pine Street");
        await _sut.AddAsync(shipment);

        // Act
        var exists = await _sut.ExistsByOrderIdAsync("ORDER-789");

        // Assert
        exists.Should().BeTrue();
    }
    
    [Fact]
    public async Task UpdateAsync_WhenMachineAllocated_ShouldPersistNewStatus()
    {
        // Arrange
        var shipment = Shipment.Create("ORDER-UPDATE", "999 Elm Street");
        await _sut.AddAsync(shipment);
        var machineId = Guid.NewGuid();
        shipment.AllocateMachine(machineId);

        // Act
        await _sut.UpdateAsync(shipment);

        // Assert
        var updated = await _sut.GetByIdAsync(shipment.ShipmentId);
        updated!.Status.Should().Be(ShipmentStatus.MachineAllocated);
        updated.MachineId.Should().Be(machineId);
    }
}