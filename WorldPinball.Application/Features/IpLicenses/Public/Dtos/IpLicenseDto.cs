
namespace WorldPinball.Application.Features.IpLicenses.Public.Dtos;

public sealed record PublisherDto(Guid PublisherId, string Name, string Slug);

public sealed record IpLicenseDto(
    Guid IpId,
    string Name,
    string Slug,
    PublisherDto? Publisher
);