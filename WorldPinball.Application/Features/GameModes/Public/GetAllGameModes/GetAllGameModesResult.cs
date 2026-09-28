using WorldPinball.Application.Features.GameModes.Public.Dtos;

namespace WorldPinball.Application.Features.GameModes.Public.GetAllGameModes;

public abstract record GetAllGameModesResult
{
    private GetAllGameModesResult() { }
    public sealed record Success(IReadOnlyList<GameModePublicDto> GameModes) : GetAllGameModesResult;
    public sealed record NotFound : GetAllGameModesResult;
}
