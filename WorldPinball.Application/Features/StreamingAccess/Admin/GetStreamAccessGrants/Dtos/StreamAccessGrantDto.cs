namespace WorldPinball.Application.Features.StreamingAccess.Admin.GetStreamAccessGrants.Dtos;

public sealed record StreamAccessGrantDto(
    Guid GrantId,
    string PrincipalType,
    Guid PrincipalId,
    string DisplayName,
    string Origin,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? ExpiresAtUtc,
    DateTime? RevokedAtUtc,
    string? RevocationReason
);