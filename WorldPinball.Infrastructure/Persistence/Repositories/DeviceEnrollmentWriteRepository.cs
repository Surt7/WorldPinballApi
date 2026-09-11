using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Repositories;

public class DeviceEnrollmentWriteRepository: IDeviceEnrollmentWriteRepository
{
    
    private readonly AppDbContext _db;
    public DeviceEnrollmentWriteRepository(AppDbContext db)
    {
        _db = db;
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