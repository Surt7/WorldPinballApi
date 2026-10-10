using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorldPinball.Api.Contracts.Matchmaking;
using WorldPinball.Application.Features.Matchmaking.Public.JoinQueue;
using WorldPinball.Application.Features.Matchmaking.Public.JoinQueue.Commands;
using WorldPinball.Application.Features.Matchmaking.Public.LeaveQueue;
using WorldPinball.Application.Features.Matchmaking.Public.LeaveQueue.Commands;

namespace WorldPinball.Api.Controllers.Public;

[ApiController]
[Route("api/v1/matchmaking")]
public class MatchmakingController(
    IJoinMatchmakingQueueHandler joinHandler,
    ILeaveMatchmakingQueueHandler leaveHandler
) : ControllerBase
{
    [HttpPost("join")]
    [EndpointSummary("Adds a player to the matchmaking queue for a machine")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Join([FromBody] JoinMatchmakingQueueRequest request, CancellationToken ct)
    {
        var command = new JoinMatchmakingQueueCommand(request.MachineId, request.PlayerId);
        var result = await joinHandler.HandleAsync(command, ct);

        return result switch
        {
            JoinMatchmakingQueueResult.Success => Ok(new { message = "Player enqueued successfully." }),
            JoinMatchmakingQueueResult.MachineNotFound notFound => NotFound(new
            {
                message = $"Machine '{notFound.MachineId}' was not found."
            }),
            _ => throw new InvalidOperationException($"Unsupported result: {result.GetType().Name}")
        };
    }

    [HttpPost("leave")]
    [EndpointSummary("Removes a player from the matchmaking queue")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Leave([FromBody] LeaveMatchmakingQueueRequest request, CancellationToken ct)
    {
        var command = new LeaveMatchmakingQueueCommand(request.MachineId, request.PlayerId);
        var result = await leaveHandler.HandleAsync(command, ct);

        return result switch
        {
            LeaveMatchmakingQueueResult.Success => Ok(new { message = "Player removed from queue." }),
            LeaveMatchmakingQueueResult.PlayerNotInQueue => NotFound(new
            {
                message = "Player was not found in matchmaking queue."
            }),
            _ => throw new InvalidOperationException($"Unsupported result: {result.GetType().Name}")
        };
    }
}
