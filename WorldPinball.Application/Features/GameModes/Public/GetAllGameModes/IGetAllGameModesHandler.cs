using WorldPinball.Application.Features.GameModes.Public.GetAllGameModes.Queries;

namespace WorldPinball.Application.Features.GameModes.Public.GetAllGameModes;

public interface IGetAllGameModesHandler
{
    Task<GetAllGameModesResult> HandleAsync(
        GetAllGameModesQuery query,
        CancellationToken cancellationToken = default);
}
