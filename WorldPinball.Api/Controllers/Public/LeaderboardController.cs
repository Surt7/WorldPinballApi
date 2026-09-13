using Microsoft.AspNetCore.Mvc;
using WorldPinball.Application.Features.Leaderboard.Public.Dtos;
using WorldPinball.Application.Features.Leaderboard.Public.Handlers;
using WorldPinball.Application.Features.Leaderboard.Public.Queries;

namespace WorldPinball.Api.Controllers.Public;

[ApiController]
[ProducesResponseType(typeof(List<LeaderboardRowDto>), StatusCodes.Status200OK)]
[Route("api/v1/leaderboard")]
public class LeaderboardController(GetLeaderboardHandler handler) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LeaderboardRowDto>>> Get(
        [FromQuery] string? ipSlug,
        [FromQuery] string? modeCode,
        [FromQuery] int limit = 50,
    CancellationToken cancellationToken = default)
    {
        var query = new GetLeaderboardQuery(ipSlug, modeCode, limit);

        var result = await handler.HandleAsync(query, cancellationToken);


        return Ok(result);
    }
}
