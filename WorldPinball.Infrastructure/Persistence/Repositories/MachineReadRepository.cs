using Microsoft.EntityFrameworkCore;
using WorldPinball.Application.Features.Machines.Dtos;
using WorldPinball.Application.Interfaces.Repositories;

namespace WorldPinball.Infrastructure.Persistence.Repositories;

public sealed class MachineReadRepository : IMachineReadRepository
{
    private readonly AppDbContext _db;

    public MachineReadRepository(AppDbContext db)
    {
        _db = db;
    }
    
    public async Task<IReadOnlyList<MachineRowDto>> GetAllMachines(CancellationToken ct)
    {
        return await _db.Machines
            .AsNoTracking()
            .Select(machine => new MachineRowDto(
                machine.MachineId,
                machine.IpLicense.Name,
                machine.DisplayName,
                machine.Status,
                machine.MachineNotes,
                machine.RegisteredAtUtc,
                machine.UpdatedAtUtc,
                machine.WorldPinballSerialNumber)).ToListAsync<MachineRowDto>(ct);
    }
    
    public async Task<MachineRowDto?> GetMachinesWithId(Guid machineId, CancellationToken ct)
    {
        return await _db.Machines
            .AsNoTracking()
            .Where(m => m.MachineId == machineId)
            .Select(machine => new MachineRowDto(
                machine.MachineId,
                machine.IpLicense.Name,
                machine.DisplayName,
                machine.Status,
                machine.MachineNotes,
                machine.RegisteredAtUtc,
                machine.UpdatedAtUtc,
                machine.WorldPinballSerialNumber)).SingleOrDefaultAsync(ct);
    }

}
