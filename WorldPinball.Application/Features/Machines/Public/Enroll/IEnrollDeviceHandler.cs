using WorldPinball.Application.Features.Machines.Public.Enroll.Commands;

namespace WorldPinball.Application.Features.Machines.Public.Enroll;

public interface IEnrollDeviceHandler
{
    Task<EnrollDeviceResult> HandleAsync(
        EnrollDeviceCommand command,
        CancellationToken ct = default
    );
}