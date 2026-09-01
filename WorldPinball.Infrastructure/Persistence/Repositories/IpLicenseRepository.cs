using Microsoft.EntityFrameworkCore;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Features.IpLicenses.Public.Dtos;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence.Repositories;

public class IpLicenseRepository : IIpLicenseRepository
{
    private readonly AppDbContext _db;
    public IpLicenseRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<IpLicenseDto>> GetAllAsync(CancellationToken cancellation)
    {
        return await _db.IpLicenses
         .AsNoTracking()
         .Select(x => new IpLicenseDto(
             x.IpId,
             x.Name,
             x.Slug,
             x.Publisher == null ? null : new PublisherDto(
                 x.Publisher.PublisherId,
                 x.Publisher.Name,
                 x.Publisher.Slug
             )
         ))
         .ToListAsync();

    }



    public async Task<IpLicense?> GetActiveBySlugAsync(string slug, CancellationToken cancellation)
    {
        return await _db.IpLicenses
            .AsNoTracking()
            .Where(x => x.Slug == slug && x.IsActive)
            .FirstOrDefaultAsync(cancellation);
    }
}
