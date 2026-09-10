using WorldPinball.Application.Features.Machines.Admin.Provision.Commands;

namespace WorldPinball.Application.Features.Machines.Admin.Provision;

public interface IProvisionMachineHandler
{
    Task<ProvisionMachineResult> HandleAsync(
        ProvisionMachineCommand command,
        CancellationToken ct = default);
}