using Microsoft.EntityFrameworkCore;
using WorldPinball.Logistics.Application.Features.GetShipment.Dtos;
using WorldPinball.Logistics.Application.Interfaces;

namespace WorldPinball.Logistics.Infrastructure.Persistence.Repositories;

public class ShipmentReadRepository(LogisticsDbContext dbContext):IShipmentReadRepository
{
    public async Task<ShipmentDto?> GetByIdAsync(Guid shipmentId, CancellationToken ct = default)
    {
        return await dbContext.Shipments
            .AsNoTracking()
            .Where(s => s.ShipmentId == shipmentId)
            .Select(s => new ShipmentDto(
                s.ShipmentId,
                s.OrderId,
                s.DestinationAddress,
                s.Status.ToString(),
                s.MachineId,
                s.TrackingNumber,
                s.CancellationReason))
            .FirstOrDefaultAsync(ct);
    }
}