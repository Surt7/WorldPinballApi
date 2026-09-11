using WorldPinball.Domain.Entities;
using WorldPinball.Application.Features.IpLicenses.Public.Dtos;

namespace WorldPinball.Application.Interfaces.Repositories;


public interface IIpLicenseReadRepository
{
    Task<IpLicense?> GetActiveBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IpLicenseDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
