using Microsoft.EntityFrameworkCore;

using WorldPinball.Application.Features.GameSessions.Public.Start.Dtos;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;


namespace WorldPinball.Infrastructure.Persistence.Repositories;

public class GameSessionReadRepository:IGameSessionReadRepository
{
    
    private readonly AppDbContext _db;

    public GameSessionReadRepository(AppDbContext db)
    {
        _db = db;
    }
    
    
        public async Task<StartGameSessionDto?> GetDeviceContextAsync(
            Guid deviceId,
            CancellationToken ct = default
        )
        {
      
         return await _db.Devices
                .AsNoTracking()
                .Where(d => d.DeviceId == deviceId)
                .Select( d => new StartGameSessionDto(
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

}