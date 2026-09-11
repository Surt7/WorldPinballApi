namespace WorldPinball.Application.Features.GameSesions.Public.Start.Commands;

public sealed record StartGameSessionCommand(
    Guid DeviceId,
    string ModeCode,
    string UserId,
    string ClientVersion
);