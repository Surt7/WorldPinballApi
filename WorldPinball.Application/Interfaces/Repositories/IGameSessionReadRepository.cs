using WorldPinball.Application.Features.GameSessions.Public.CreateGameSession.Dtos;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Interfaces.Repositories;

public interface IGameSessionReadRepository
{
    Task<CreateGameSessionDto?> GetDeviceContextAsync(
        Guid deviceId,
        CancellationToken cancellationToken = default
    );

    Task<GameMode?> GetActiveModeByCodeAsync(
        string modeCode,
        CancellationToken cancellationToken = default
    );

    Task<GameSession?> GetSessionForLobbyAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default
    );
}
