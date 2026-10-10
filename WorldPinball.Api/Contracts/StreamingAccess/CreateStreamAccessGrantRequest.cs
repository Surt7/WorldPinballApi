using WorldPinball.Domain.Enums;

namespace WorldPinball.Api.Contracts.StreamingAccess;

public sealed record CreateStreamAccessGrantRequest(
    PrincipalType PrincipalType,
    Guid PrincipalId,
    string DisplayName,
    GrantOrigin Origin = GrantOrigin.Whitelist,
    DateTime? ExpiresAtUtc = null
);
