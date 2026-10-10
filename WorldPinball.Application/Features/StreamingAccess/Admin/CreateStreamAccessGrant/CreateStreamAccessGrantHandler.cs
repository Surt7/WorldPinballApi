using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.StreamingAccess.Admin.CreateStreamAccessGrant.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Application.Features.StreamingAccess.Admin.CreateStreamAccessGrant;

public sealed class CreateStreamAccessGrantHandler(
    IStreamAccessGrantRepository repository,
    TimeProvider timeProvider,
    ILogger<CreateStreamAccessGrantHandler> logger
) : ICreateStreamAccessGrantHandler
{
    private static readonly EventId GrantCreated = new(3420, nameof(GrantCreated));
    private static readonly EventId GrantAlreadyExists = new(3421, nameof(GrantAlreadyExists));
    private static readonly EventId InvalidGrantData = new(3422, nameof(InvalidGrantData));

    public async Task<CreateStreamAccessGrantResult> HandleAsync(
        CreateStreamAccessGrantCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.PrincipalId == Guid.Empty)
        {
            logger.LogWarning(InvalidGrantData, "Create grant failed: PrincipalId cannot be empty.");
            return new CreateStreamAccessGrantResult.InvalidData("PrincipalId cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(command.DisplayName))
        {
            logger.LogWarning(InvalidGrantData, "Create grant failed: DisplayName cannot be empty.");
            return new CreateStreamAccessGrantResult.InvalidData("DisplayName cannot be empty.");
        }

        // Check if an active grant already exists for this principal
        var existing = await repository.FindActiveGrantAsync(
            command.PrincipalType,
            command.PrincipalId,
            cancellationToken);

        if (existing is not null)
        {
            logger.LogWarning(GrantAlreadyExists, "An active grant already exists for {PrincipalType} {PrincipalId}",
                command.PrincipalType, command.PrincipalId);
            return new CreateStreamAccessGrantResult.AlreadyExists(existing.GrantId);
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        var grant = new StreamAccessGrant(
            command.PrincipalType,
            command.PrincipalId,
            command.DisplayName,
            command.Origin,
            nowUtc,
            command.ExpiresAtUtc,
            command.GrantedByUserId
        );

        await repository.AddAsync(grant, cancellationToken);

        logger.LogInformation(GrantCreated, "Created stream grant {GrantId} for {DisplayName} ({Origin})",
            grant.GrantId, grant.DisplayName, grant.Origin);

        return new CreateStreamAccessGrantResult.Success(grant.GrantId);
    }
}