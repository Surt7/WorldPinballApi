using WorldPinball.Application.Features.Machines.Admin.Commands;

namespace WorldPinball.Application.Features.Machines.Admin;

public interface IProvisionMachineHandler
{
    Task<ProvisionMachineResult> HandleAsync(
        ProvisionMachineCommand command,
        CancellationToken ct = default);
}