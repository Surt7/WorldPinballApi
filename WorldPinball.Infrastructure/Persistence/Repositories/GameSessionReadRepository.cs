using Microsoft.EntityFrameworkCore;
using WorldPinball.Application.Features.GameSessions.Public.CreateGameSession.Dtos;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Infrastructure.Persistence.Repositories;

public class GameSessionReadRepository : IGameSessionReadRepository
{
    private readonly AppDbContext _db;

    public GameSessionReadRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CreateGameSessionDto?> GetDeviceContextAsync(
        Guid deviceId,
        CancellationToken ct = default
    )
    {
        return await _db.Devices
            .AsNoTracking()
            .Where(d => d.DeviceId == deviceId)
            .Select(d => new CreateGameSessionDto(
                d.DeviceId,
                d.Status,
                _db.MachineDevices
                    .Where(m => m.DeviceId == d.DeviceId && m.IsActive)
                    .Select(md => (Guid?)md.MachineId)
                    .FirstOrDefault()
            )).FirstOrDefaultAsync(ct);
    }

    public async Task<GameMode?> GetActiveModeByCodeAsync(
        string modeCode,
        CancellationToken ct = default
    )
    {
        return await _db.GameModes.AsNoTracking().Where(m => m.Code == modeCode).FirstOrDefaultAsync(ct);
    }

    public async Task<GameSession?> GetSessionForLobbyAsync(
        Guid sessionId,
        CancellationToken ct = default
    )
    {
        return await _db.GameSessions
            .AsNoTracking()
            .Where(gs => gs.SessionId == sessionId)
            .Include(gs => gs.Players)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Guid>> GetActiveSessionIdsAsync(CancellationToken cancellationToken = default)
    {
        var terminalStates = new[]
        {
            GameSessionState.Ended,
            GameSessionState.Cancelled,
            GameSessionState.Abandoned,
            GameSessionState.Aborted
        };
        return await _db.GameSessions
            .AsNoTracking()
            .Where(gs => !terminalStates.Contains(gs.State))
            .OrderByDescending(gs => gs.StartedAtUtc)
            .Select(gs => gs.SessionId)
            .ToListAsync(cancellationToken);
    }
}