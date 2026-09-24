using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Interfaces.Repositories;


public interface IGameSessionWriteRepository
{
    Task SaveSessionAsync(GameSession session, CancellationToken cancellationToken = default);
    Task UpdateSessionAsync(GameSession session, CancellationToken cancellationToken = default);
    Task<GameSession?> GetSessionWithMachineAsync(Guid sessionId, CancellationToken cancellationToken = default);
    
    Task SaveScoreEntryAsync(GameSession session, ScoreEntry scoreEntry, CancellationToken cancellationToken = default);
}



