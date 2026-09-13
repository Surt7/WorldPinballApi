
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;


namespace WorldPinball.Infrastructure.Persistence.Repositories;

public class GameSessionWriteRepository:IGameSessionWriteRepository
{
    
    private readonly AppDbContext _db;

    public GameSessionWriteRepository(AppDbContext db)
    {
        _db = db;
    }

        public async Task SaveSessionAsync(GameSession session, CancellationToken cancellationToken = default)
        {
            await _db.GameSessions.AddAsync(session, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }
}