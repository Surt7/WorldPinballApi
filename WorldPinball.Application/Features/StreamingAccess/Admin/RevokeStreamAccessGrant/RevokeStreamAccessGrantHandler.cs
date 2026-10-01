using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.StreamingAccess.Admin.RevokeStreamAccessGrant.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.StreamingAccess.Admin.RevokeStreamAccessGrant;

public sealed class RevokeStreamAccessGrantHandler(
    IStreamAccessGrantRepository repository,
    TimeProvider timeProvider,
    ILogger<RevokeStreamAccessGrantHandler> logger
) : IRevokeStreamAccessGrantHandler
{
    private static readonly EventId GrantRevoked = new(3410, nameof(GrantRevoked));
    private static readonly EventId GrantNotFound = new(3411, nameof(GrantNotFound));
    private static readonly EventId GrantAlreadyRevoked = new(3412, nameof(GrantAlreadyRevoked));
    private static readonly EventId InvalidRevocationReason = new(3413, nameof(InvalidRevocationReason));

    public async Task<RevokeStreamAccessGrantResult> HandleAsync(
        RevokeStreamAccessGrantCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. Friction-by-design: mandatory revocation reason
        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            logger.LogWarning(InvalidRevocationReason, "Revocation failed: reason cannot be empty for Grant {GrantId}.", command.GrantId);
            return new RevokeStreamAccessGrantResult.InvalidReason("A revocation reason is required.");
        }

        // 2. Fetch the grant
        var grant = await repository.GetByIdAsync(command.GrantId, cancellationToken);
        if (grant is null)
        {
            logger.LogWarning(GrantNotFound, "Grant {GrantId} not found for revocation.", command.GrantId);
            return new RevokeStreamAccessGrantResult.NotFound(command.GrantId);
        }

        // 3. Idempotency check: already revoked
        if (grant.Status is GrantStatus.Revoked)
        {
            logger.LogWarning(GrantAlreadyRevoked, "Grant {GrantId} is already revoked.", command.GrantId);
            return new RevokeStreamAccessGrantResult.AlreadyRevoked(command.GrantId);
        }

        // 4. Apply domain business logic
        var revokedAt = command.RevokedAtUtc ?? timeProvider.GetUtcNow().UtcDateTime;
        grant.Revoke(command.Reason, revokedAt);

        await repository.UpdateAsync(grant, cancellationToken);

        logger.LogInformation(GrantRevoked, "Grant {GrantId} successfully revoked. Reason: {Reason}", command.GrantId, command.Reason);

        return new RevokeStreamAccessGrantResult.Success(grant.GrantId);
    }
}