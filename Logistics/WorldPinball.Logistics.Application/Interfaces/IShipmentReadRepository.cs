using WorldPinball.Logistics.Application.Features.GetShipment.Dtos;

namespace WorldPinball.Logistics.Application.Interfaces;

public interface IShipmentReadRepository
{
    Task<ShipmentDto?> GetByIdAsync(Guid shipmentId, CancellationToken ct = default);
}