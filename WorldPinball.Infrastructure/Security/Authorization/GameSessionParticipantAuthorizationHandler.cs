using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Security.Authorization;

public sealed class GameSessionParticipantAuthorizationHandler 
    : AuthorizationHandler<GameSessionParticipantRequirement, GameSession>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        GameSessionParticipantRequirement requirement,
        GameSession resource)
    {
        var rawUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Verify user ID claim matches any registered session player
        if (Guid.TryParse(rawUserId, out var callerId) &&
            (resource.MachineId == callerId || resource.Players.Any(p => p.UserId == callerId)))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}