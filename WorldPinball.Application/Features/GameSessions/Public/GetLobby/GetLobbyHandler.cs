using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.GameSessions.Public.GetLobby.Dtos;
using WorldPinball.Application.Features.GameSessions.Public.GetLobby.Queries;
using WorldPinball.Application.Interfaces.Repositories;

namespace WorldPinball.Application.Features.GameSessions.Public.GetLobby;

public sealed class GetLobbyHandler(
    IGameSessionWriteRepository writeRepository,
    ILogger<GetLobbyHandler> logger
    ):IGetLobbyHandler
{
    
    private static readonly EventId SessionNotFound = new EventId(2700, nameof(SessionNotFound));
    
    public async Task<GetLobbyResult> HandleAsync(GetLobbyQuery query, CancellationToken cancellationToken = default)
    {
        var session = await writeRepository.GetSessionWithMachineAsync(
            query.SessionId,
            cancellationToken);

        
        if (session is null)
        {
            logger.LogWarning(
                SessionNotFound,
                "Game session {SessionId} could not be found.",
                query.SessionId);
            return new GetLobbyResult.SessionNotFound(query.SessionId);
        }
        
        
        var slots = new List<LobbySlotDto>(4);
        for (int i = 1; i <= 4; i++)
        {
            var player = session.Players.FirstOrDefault(p => p.SlotNumber == i);
            slots.Add(new LobbySlotDto(
                SlotNumber: i,
                IsOccupied: player is not null,
                UserId: player?.UserId,
                GuestInitials: player?.GuestInitials
            ));
        }
        
        var lobbyDto = new SessionLobbyDto(
            session.SessionId,
            session.State.ToString(),
            slots);
        return new GetLobbyResult.Success(lobbyDto);
    }
}