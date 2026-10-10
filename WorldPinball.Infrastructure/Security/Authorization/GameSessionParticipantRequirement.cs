using Microsoft.AspNetCore.Authorization;

namespace WorldPinball.Infrastructure.Security.Authorization;

public sealed record GameSessionParticipantRequirement : IAuthorizationRequirement;