using WorldPinball.Domain.Entities;

namespace WorldPinball.Application.Interfaces.Repositories;

public interface IMachineWriteRepository
{
    Task<bool> SerialExistsAsync(
        string serialNumber,
        CancellationToken ct = default);

    Task<bool> SameIdempotencyKeyExistsAsync(
        Guid idempotencyKey,
        CancellationToken ct = default);
    
    
Task AddProvisioningAsync(
        Machine machine,
        EnrollmentToken enrollmentToken,
        CancellationToken ct = default);

Task<Machine?> GetByIdAsync(
    Guid machineId,
    CancellationToken ct = default);

Task AddEnrollmentTokenAsync(
    EnrollmentToken enrollmentToken,
    CancellationToken ct = default);
}
