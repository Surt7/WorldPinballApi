using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.StreamingAccess.Admin.CreateStreamAccessGrant.Commands;

public sealed record CreateStreamAccessGrantCommand(
    PrincipalType PrincipalType,
    Guid PrincipalId,
    string DisplayName,
    GrantOrigin Origin,
    DateTime? ExpiresAtUtc = null,
    Guid? GrantedByUserId = null
);