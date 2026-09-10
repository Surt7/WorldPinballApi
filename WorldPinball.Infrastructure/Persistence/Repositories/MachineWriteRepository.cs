using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Repositories;

public sealed class MachineWriteRepository(AppDbContext db) : IMachineWriteRepository
{
    
    public Task<bool> SerialExistsAsync(
        string serialNumber,
        CancellationToken ct = default)
    {
        return db.Machines.AnyAsync(
            machine => machine.WorldPinballSerialNumber == serialNumber,
            ct);
    }
    public Task<bool> SameIdempotencyKeyExistsAsync(
        Guid idempotencyKey,
        CancellationToken ct = default)
    {
         return   db.Machines.AnyAsync(
             machine => machine.IdempotencyKey == idempotencyKey,
             ct);;
    }
    
    
    public async Task AddProvisioningAsync(
        Machine machine,
        EnrollmentToken enrollmentToken,
        CancellationToken ct = default)
    {
        db.Machines.Add(machine);
        db.EnrollmentTokens.Add(enrollmentToken);

        await db.SaveChangesAsync(ct);
    }
}
