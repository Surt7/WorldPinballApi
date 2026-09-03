using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WorldPinball.Tests.Integration;

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(
        options,
        logger,
        encoder)
{
    public const string SchemeName = "Test";
    public const string UserHeaderName = "X-Test-User";
    public const string RoleHeaderName = "X-Test-Role";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Missing this test-only header represents an unauthenticated request.
        if (!Request.Headers.TryGetValue(UserHeaderName, out var userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new("sub", userId.ToString())
        };

        if (Request.Headers.TryGetValue(RoleHeaderName, out var role))
        {
            claims.Add(new Claim("roles", role.ToString()));
        }

        // A non-empty authentication type marks the identity as authenticated.
        // The role type tells IsInRole() which claim contains roles.
        var identity = new ClaimsIdentity(
            claims,
            authenticationType: SchemeName,
            nameType: "sub",
            roleType: "roles");

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        // The authentication middleware takes this principal and assigns it to HttpContext.User.
        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}
