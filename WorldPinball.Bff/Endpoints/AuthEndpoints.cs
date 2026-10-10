using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;


namespace WorldPinball.Bff.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        // Triggers OIDC Challenge to redirect user to Keycloak login page
        group.MapGet("/login", (string? returnUrl) =>
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl
            };
            return Results.Challenge(props, [OpenIdConnectDefaults.AuthenticationScheme]);
        });

        group.MapGet("/user", (ClaimsPrincipal user) =>
        {
            if (user.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
            var name = user.Identity.Name ?? user.FindFirst("preferred_username")?.Value;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();


            return Results.Ok(new { Subject = sub, Username = name, Roles = roles });

        });
        return app;
    }
}