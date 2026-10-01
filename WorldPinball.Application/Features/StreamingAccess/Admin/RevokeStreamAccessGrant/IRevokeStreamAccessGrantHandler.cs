using WorldPinball.Application.Features.StreamingAccess.Admin.RevokeStreamAccessGrant.Commands;

namespace WorldPinball.Application.Features.StreamingAccess.Admin.RevokeStreamAccessGrant;

public interface IRevokeStreamAccessGrantHandler
{
    Task<RevokeStreamAccessGrantResult> HandleAsync(
        RevokeStreamAccessGrantCommand command,
        CancellationToken cancellationToken = default);
}