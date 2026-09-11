using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Interfaces.Repositories;

public sealed record DeviceSessionContext(
    Guid DeviceId,
    DeviceStatus Status,
    Guid? ActiveMachineId
);

public interface IGameSessionReadRepository
{
    Task<DeviceSessionContext?> GetDeviceContextAsync(
        Guid deviceId,
        CancellationToken cancellationToken = default
    );

    Task<GameMode?> GetActiveModeByCodeAsync(
        string modeCode,
        CancellationToken cancellationToken = default
    );
}

public interface IGameSessionWriteRepository
{
    Task SaveSessionAsync(
        GameSession session,
        CancellationToken cancellationToken = default
    );
}



