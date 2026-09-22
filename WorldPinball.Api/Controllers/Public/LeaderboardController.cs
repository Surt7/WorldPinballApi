using Microsoft.AspNetCore.Mvc;
using WorldPinball.Api.Contracts.Leaderboard;
using WorldPinball.Application.Features.Leaderboard.Public;
using WorldPinball.Application.Features.Leaderboard.Public.Handlers;
using WorldPinball.Application.Features.Leaderboard.Public.Queries;

namespace WorldPinball.Api.Controllers.Public;

[ApiController]
[Route("api/v1/leaderboard")]
public class LeaderboardController(GetLeaderboardHandler handler) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Get leaderboard")]
    [EndpointDescription("Retrieve the leaderboard for a specific pinball machine and game mode")]
    [ProducesResponseType(typeof(GetLeaderboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetLeaderboardResponse>> Get(
        [FromQuery] GetLeaderboardRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLeaderboardQuery(
            request.IpId,
            request.ModeId,
            request.IpSlug,
            request.ModeCode,
            request.Limit);

        var result = await handler.HandleAsync(query, cancellationToken);

        return result switch
        {
            GetLeaderboardResult.Success success => Ok(new GetLeaderboardResponse(success.Rows)),
            GetLeaderboardResult.InvalidParameters invalid => BadRequest(new { message = invalid.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
