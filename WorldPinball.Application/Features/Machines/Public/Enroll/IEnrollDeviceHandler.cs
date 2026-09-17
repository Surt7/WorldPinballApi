using WorldPinball.Application.Features.Machines.Public.Enroll.Commands;

namespace WorldPinball.Application.Features.Machines.Public;

public interface IEnrollDeviceHandler
{
    Task<EnrollDeviceResult> HandleAsync(
        EnrollDeviceCommand command,
        CancellationToken ct = default
    );
}