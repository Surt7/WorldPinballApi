using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.GameModes.Public.GetAllGameModes.Queries;
using WorldPinball.Application.Interfaces.Repositories;

namespace WorldPinball.Application.Features.GameModes.Public.GetAllGameModes;

public sealed class GetAllGameModesHandler(
    IGameModeRepository gameModeRepository,
    ILogger<GetAllGameModesHandler> logger
) : IGetAllGameModesHandler
{
    private static readonly EventId FetchingGameModes = new(3200, nameof(FetchingGameModes));
    private static readonly EventId NoGameModesFound = new(3201, nameof(NoGameModesFound));

    public async Task<GetAllGameModesResult> HandleAsync(
        GetAllGameModesQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(FetchingGameModes, "Fetching all game modes.");
        var gameModes = await gameModeRepository.GetGameModeAsync(cancellationToken);

        if (gameModes.Count == 0)
        {
            logger.LogWarning(NoGameModesFound, "No game modes found in the system.");
            return new GetAllGameModesResult.NotFound();
        }

        return new GetAllGameModesResult.Success(gameModes);
    }
}
