using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorldPinball.Application.Interfaces.Repositories;


namespace WorldPinball.Api.Controllers.Public
{
    [Authorize]
    [Route("api/v1/game-modes")]
    [ApiController]
    public class GameModesController(IGameModeRepository gameModeRepository) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var allGameModes = await gameModeRepository.GetGameModeAsync(cancellationToken);

            return Ok(allGameModes);
        }
    }
}
