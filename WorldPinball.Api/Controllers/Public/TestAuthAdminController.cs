using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WorldPinball.Api.Controllers.Public
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        [HttpGet("auth")]
        [Authorize]
        public IActionResult Auth() => Ok("auth ok");

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult Admin() => Ok("admin ok");


        [Authorize]
        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            var sub =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("sub");

            var email =
                User.FindFirstValue(ClaimTypes.Email) ??
                User.FindFirstValue("email");

            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "roles" || c.Type.EndsWith("/role"))
                .Select(c => c.Value)
                .Distinct()
                .ToArray();

            return Ok(new { sub, email, roleClaims = roles });
        }

    }


}
