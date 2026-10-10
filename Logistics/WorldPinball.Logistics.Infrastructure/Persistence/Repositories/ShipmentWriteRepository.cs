using Microsoft.EntityFrameworkCore;
using WorldPinball.Logistics.Application.Interfaces;
using WorldPinball.Logistics.Domain.Entities;

namespace WorldPinball.Logistics.Infrastructure.Persistence.Repositories;

public class ShipmentWriteRepository(LogisticsDbContext dbContext):IShipmentWriteRepository
{
    public async Task<bool> ExistsByOrderIdAsync(string orderId, CancellationToken ct = default)
    {
        return await dbContext.Shipments.AnyAsync(s => s.OrderId == orderId, ct);
    }

    public async Task AddAsync(Shipment shipment, CancellationToken ct = default)
    {
        await dbContext.Shipments.AddAsync(shipment, ct);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<Shipment?> GetByIdAsync(Guid shipmentId, CancellationToken ct = default)
    {
        return await dbContext.Shipments.FirstOrDefaultAsync(s => s.ShipmentId == shipmentId, ct);
    }

    public async Task UpdateAsync(Shipment shipment, CancellationToken ct = default)
    {
        dbContext.Shipments.Update(shipment);
        await dbContext.SaveChangesAsync(ct);
    }
}