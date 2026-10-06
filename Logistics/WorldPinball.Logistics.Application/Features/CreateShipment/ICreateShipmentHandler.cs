using WorldPinball.Logistics.Application.Features.CreateShipment.Commands;

namespace WorldPinball.Logistics.Application.Features.CreateShipment;

public interface ICreateShipmentHandler
{
    Task<CreateShipmentResult> HandleAsync(CreateShipmentCommand command, CancellationToken ct = default);
}