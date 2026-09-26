using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WorldPinball.Infrastructure.Persistence.Repositories;

public class DeviceEnrollmentReadRepository : IDeviceEnrollmentReadRepository
{
    private readonly AppDbContext _db;


    public DeviceEnrollmentReadRepository(AppDbContext db)
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


}
