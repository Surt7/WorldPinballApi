namespace WorldPinball.Application.Features.GameSessions.Public.CreateGameSession.Commands;

public sealed record CreateGameSessionCommand(
    Guid DeviceId,
    string ModeCode,
    string ClientVersion
);
