using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.GameSessions.Public.Start.Dtos;


public sealed record StartGameSessionDto(
    Guid DeviceId,
    DeviceStatus Status,
    Guid? ActiveMachineId
);