using WorldPinball.Application.Features.GameSessions.Public.CreateGameSession.Dtos;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Application.Features.GameSessions.Public.CreateGameSession.Profile;

public class CreateGameSessionMappingProfile : AutoMapper.Profile
{
    public CreateGameSessionMappingProfile()
    {
        CreateMap<GameSession, CreateGameSessionDto>();
    }
}