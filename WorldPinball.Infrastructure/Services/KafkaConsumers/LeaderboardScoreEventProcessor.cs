using System.Text.Json;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Events;

namespace WorldPinball.Infrastructure.Services.KafkaConsumers;

public interface ILeaderboardScoreEventProcessor
{
    Task ProcessMessageAsync(string jsonPayload, CancellationToken ct = default);
}


public class LeaderboardScoreEventProcessor(
    IRealtimeLeaderboardService leaderboardService
    ):ILeaderboardScoreEventProcessor
{
    public async Task ProcessMessageAsync(string jsonPayload, CancellationToken ct = default)
    {
        
        ScoreSubmittedDomainEvent? evt = null;
        
    
        try
        {
            evt = JsonSerializer.Deserialize<ScoreSubmittedDomainEvent>(jsonPayload);
        }
        catch(JsonException)
        {
            return;
        }
        
        if (evt is null) return;
        
        if (evt.ScoreEntryId == Guid.Empty || evt.IpId == Guid.Empty || evt.Score < 0)
        {
            return;
        }
        
        await leaderboardService.AddScoreAsync(
            evt.IpId, evt.ModeId,evt.ScoreEntryId, evt.Score, ct );


    }
    
}