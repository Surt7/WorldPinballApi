using WorldPinball.Logistics.Domain.Entities;

namespace WorldPinball.Logistics.Application.Interfaces;

public interface IShipmentWriteRepository
{
    Task<bool> ExistsByOrderIdAsync(string orderId, CancellationToken ct = default);
    
    Task AddAsync(Shipment shipment, CancellationToken ct = default);
    
    Task<Shipment?> GetByIdAsync(Guid shipmentId, CancellationToken ct = default);
    
    Task UpdateAsync(Shipment shipment, CancellationToken ct = default);
}