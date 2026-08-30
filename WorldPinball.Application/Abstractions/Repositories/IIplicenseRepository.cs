using WorldPinball.Domain.Entities;
using WorldPinball.Application.Features.IpLicenses.Public.Dtos;

namespace WorldPinball.Application.Abstractions.Repositories;


public interface IIpLicenseRepository
{
    Task<IpLicense?> GetActiveBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IpLicenseDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
