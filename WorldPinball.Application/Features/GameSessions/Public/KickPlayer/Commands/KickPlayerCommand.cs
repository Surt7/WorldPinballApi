namespace WorldPinball.Application.Features.GameSessions.Public.KickPlayer.Commands;

public sealed record KickPlayerCommand(
    Guid SessionId,
    int SlotNumber,
    string? Reason = null
    );