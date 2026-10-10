using WorldPinball.Logistics.Application.Features.GetShipment.Queries;

namespace WorldPinball.Logistics.Application.Features.GetShipment;

public interface IGetShipmentByIdHandler
{
    Task<GetShipmentByIdResult> HandleAsync(GetShipmentByIdQuery query, CancellationToken ct = default);
}