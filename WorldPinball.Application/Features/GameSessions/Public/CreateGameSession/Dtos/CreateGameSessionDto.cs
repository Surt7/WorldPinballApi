using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.GameSessions.Public.CreateGameSession.Dtos;

public sealed record CreateGameSessionDto(
    Guid DeviceId,
    DeviceStatus Status,
    Guid? ActiveMachineId
);