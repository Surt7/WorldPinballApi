using Microsoft.Extensions.Logging;
using WorldPinball.Logistics.Application.Features.GetShipment.Queries;
using WorldPinball.Logistics.Application.Interfaces;

namespace WorldPinball.Logistics.Application.Features.GetShipment;

public sealed class GetShipmentByIdHandler(
    IShipmentReadRepository repository,
    ILogger<GetShipmentByIdHandler> logger) : IGetShipmentByIdHandler
{
    public async Task<GetShipmentByIdResult> HandleAsync(
        GetShipmentByIdQuery query, 
        CancellationToken ct = default)
    {
        var shipment = await repository.GetByIdAsync(query.ShipmentId, ct);
        if (shipment is null)
        {
            logger.LogWarning("Shipment {ShipmentId} not found", query.ShipmentId);
            return new GetShipmentByIdResult.NotFound(query.ShipmentId);
        }

        return new GetShipmentByIdResult.Success(shipment);
    }
}
