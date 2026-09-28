using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorldPinball.Application.Features.GameModes.Public.GetAllGameModes;
using WorldPinball.Application.Features.GameModes.Public.GetAllGameModes.Queries;

namespace WorldPinball.Api.Controllers.Public;

[Authorize]
[Route("api/v1/game-modes")]
[ApiController]
public class GameModesController(
    IGetAllGameModesHandler getAllGameModesHandler
) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Returns all available game modes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await getAllGameModesHandler.HandleAsync(new GetAllGameModesQuery(), ct);

        return result switch
        {
            GetAllGameModesResult.Success success => Ok(success.GameModes),
            GetAllGameModesResult.NotFound => NotFound(new
            {
                message = "No game modes found."
            }),
            _ => throw new InvalidOperationException($"Unsupported result: {result.GetType().Name}")
        };
    }
}
