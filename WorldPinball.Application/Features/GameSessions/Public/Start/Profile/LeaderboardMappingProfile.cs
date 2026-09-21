using AutoMapper;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Entities;


namespace WorldPinball.Application.Features.GameSessions.Public.Start.Profile;




public sealed class LeaderboardMappingProfile : AutoMapper.Profile
{
    public LeaderboardMappingProfile()
    {
        CreateMap<ScoreEntry, LeaderboardEntryDto>();
    }
}