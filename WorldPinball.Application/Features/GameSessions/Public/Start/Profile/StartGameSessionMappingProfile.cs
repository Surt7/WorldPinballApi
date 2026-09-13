using WorldPinball.Application.Features.GameSessions.Public.Start.Dtos;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Application.Features.GameSessions.Public.Start.Profile;

public class StartGameSessionMappingProfile: AutoMapper.Profile
{

    public StartGameSessionMappingProfile()
    {
        CreateMap<GameSession, StartGameSessionDto>();
    }
    
    
}