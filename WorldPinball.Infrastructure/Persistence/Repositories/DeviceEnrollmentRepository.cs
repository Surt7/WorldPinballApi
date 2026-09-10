using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using WorldPinball.Application.Features.Machines.Public.Dto;

namespace WorldPinball.Infrastructure.Persistence.Repositories;

public class DeviceEnrollmentRepository : IDeviceEnrollmentRepository
{
    private readonly AppDbContext _db;


    public DeviceEnrollmentRepository(AppDbContext db)
    {
        _db = db;
    }


    public async Task<EnrollmentToken?> GetTokenByHashAsync(string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return await _db.EnrollmentTokens
            .SingleOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    }

    public async Task<bool> DeviceSerialExistsAsync(string serialPhysical, CancellationToken cancellationToken = default)
    {
        return await _db.Devices
            .AnyAsync(d => d.SerialPhysical == serialPhysical, cancellationToken);
    }

    public async Task SaveEnrollmentAsync(Device device, MachineDevice machineDevice, EnrollmentToken enrollmentToken,
        CancellationToken cancellationToken = default)
    {
        _db.Devices.Add(device);
        _db.MachineDevices.Add(machineDevice);
        _db.EnrollmentTokens.Update(enrollmentToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}