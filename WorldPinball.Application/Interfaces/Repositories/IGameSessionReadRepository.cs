
using WorldPinball.Application.Features.GameSessions.Public.Start.Dtos;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Interfaces.Repositories;



public interface IGameSessionReadRepository
{
 Task<StartGameSessionDto?> GetDeviceContextAsync(
        Guid deviceId,
        CancellationToken cancellationToken = default
    );

  Task<GameMode?> GetActiveModeByCodeAsync(
        string modeCode,
        CancellationToken cancellationToken = default
    );

 
}





