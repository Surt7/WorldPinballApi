using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.GameSessions.Public.CreateGameSession.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.GameSessions.Public.CreateGameSession;

public sealed class CreateGameSessionHandler(
    IGameSessionReadRepository readRepository,
    IGameSessionWriteRepository writeRepository,
    ILogger<CreateGameSessionHandler> logger,
    TimeProvider timeProvider
) : ICreateGameSessionHandler
{
    private static readonly EventId DeviceNotFound = new EventId(2300, nameof(DeviceNotFound));
    private static readonly EventId DeviceNotActive = new EventId(2301, nameof(DeviceNotActive));
    private static readonly EventId MachineNotAttached = new EventId(2302, nameof(MachineNotAttached));
    private static readonly EventId ModeNotFound = new EventId(2303, nameof(ModeNotFound));

    public async Task<CreateGameSessionResult> HandleAsync(
        CreateGameSessionCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        
        var device = await readRepository.GetDeviceContextAsync(
            command.DeviceId,
            cancellationToken
        );

        if (device is null)
        {
            logger.LogWarning(
                DeviceNotFound,
                "Device {DeviceId} could not be found.",
                command.DeviceId);
            return new CreateGameSessionResult.DeviceNotFound(command.DeviceId);
        }

        if (device.Status is not DeviceStatus.Active)
        {
            logger.LogWarning(
                DeviceNotActive,
                "Device {DeviceId} not active.",
                command.DeviceId
            );   
            return new CreateGameSessionResult.DeviceNotActive(command.DeviceId);
        }

        if (device.ActiveMachineId is null)
        {
            logger.LogWarning(
                MachineNotAttached,
                "Device {DeviceId} has no active machine attached.",
                command.DeviceId
            );
            return new CreateGameSessionResult.MachineNotAttached(command.DeviceId);
        }

        var mode = await readRepository.GetActiveModeByCodeAsync(command.ModeCode, cancellationToken);
        if (mode is null)
        {
            logger.LogWarning(
                ModeNotFound,
                "Game mode {ModeCode} could not be found or is inactive.",
                command.ModeCode
            );
            return new CreateGameSessionResult.ModeNotFound(command.ModeCode);
        }

        var nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        var session = new GameSession(
            device.ActiveMachineId.Value,
            command.DeviceId,
            mode.ModeId,
            command.ClientVersion,
            nonce,
            nowUtc
        );

        await writeRepository.SaveSessionAsync(session, cancellationToken);

        return new CreateGameSessionResult.Success(session.SessionId, nonce, nowUtc);
    }
}
