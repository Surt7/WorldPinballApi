using WorldPinball.Application.Features.Machines.Dtos;

namespace WorldPinball.Application.Interfaces.Repositories;

public interface IMachineReadRepository
{
    Task<IReadOnlyList<MachineRowDto>> GetAllMachines(
        CancellationToken ct = default);

    Task<MachineRowDto?> GetMachinesWithId(
        Guid machineId,
        CancellationToken ct = default);
}
