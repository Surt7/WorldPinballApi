using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.Machines.Admin.CreateOperatorToken.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;


namespace WorldPinball.Application.Features.Machines.Admin.CreateOperatorToken;

public sealed class CreateOperatorTokenHandler(
    IOperatorTokenWriteRepository repository,
    TimeProvider timeProvider,
    ILogger<CreateOperatorTokenHandler> logger
    ):ICreateOperatorTokenHandler
{
    private static readonly EventId OperatorTokenCreated =
        new(2060, nameof(OperatorTokenCreated));
    
    public async Task<CreateOperatorTokenResult> HandleAsync(
        CreateOperatorTokenCommand command,
        CancellationToken ct = default)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        var randomBytes = RandomNumberGenerator.GetBytes(24);
        var rawToken = "wpt_live_" + Convert.ToHexString(randomBytes).ToLowerInvariant();

        DateTime? expiresAtUtc = command.ValidityDays is > 0
            ? nowUtc.AddDays(command.ValidityDays.Value)
            : null;

        var token = new OperatorToken(
            name: command.Name,
            rawToken: rawToken,
            scopes: command.Scopes,
            issuedByUserId: command.IssuedByUserId,
            expiresAtUtc: expiresAtUtc,
            nowUtc: nowUtc);
        
        await repository.AddAsync(token, ct);
        
        
        logger.LogInformation(
            OperatorTokenCreated,
            "Operator token created: {TokenName} ({Prefix}) with scopes: {Scopes} by user {UserId}",
            token.Name,
            token.Prefix,
            token.Scopes,
            command.IssuedByUserId);
        
        return new CreateOperatorTokenResult.Success(
            token.TokenId,
            token.Name,
            rawToken,
            token.Prefix,
            token.Scopes,
            token.ExpiresAtUtc);
    }
}