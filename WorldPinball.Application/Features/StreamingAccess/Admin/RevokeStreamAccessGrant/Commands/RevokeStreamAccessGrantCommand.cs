namespace WorldPinball.Application.Features.StreamingAccess.Admin.RevokeStreamAccessGrant.Commands;

public sealed record RevokeStreamAccessGrantCommand(
    Guid GrantId,
    string Reason,
    DateTime? RevokedAtUtc = null
);