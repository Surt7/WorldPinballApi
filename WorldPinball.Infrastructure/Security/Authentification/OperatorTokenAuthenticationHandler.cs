using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorldPinball.Application.Security;

namespace WorldPinball.Infrastructure.Security.Authentification;

public sealed class OperatorTokenAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOperatorTokenValidator tokenValidator) 
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "OperatorToken";
    public const string HeaderName = "X-Operator-Token";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 1. Extract raw token from X-Operator-Token or Authorization header
        string? rawToken = null;

        if (Request.Headers.TryGetValue(HeaderName, out var headerValues))
        {
            rawToken = headerValues.FirstOrDefault();
        }
        else if (Request.Headers.TryGetValue("Authorization", out var authValues))
        {
            var authHeader = authValues.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    rawToken = authHeader["Bearer ".Length..].Trim();
                }
                else if (authHeader.StartsWith("Operator ", StringComparison.OrdinalIgnoreCase))
                {
                    rawToken = authHeader["Operator ".Length..].Trim();
                }
            }
        }

        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return AuthenticateResult.NoResult();
        }

        // 2. Validate token against DB and cache
        var token = await tokenValidator.ValidateAsync(rawToken, Context.RequestAborted);
        if (token == null)
        {
            return AuthenticateResult.Fail("Invalid, expired, or revoked operator token.");
        }

        // 3. Build ClaimsPrincipal with scopes
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, token.TokenId.ToString()),
            new(ClaimTypes.Name, token.Name),
            new("operator_prefix", token.Prefix)
        };

        // Exploit clean IEnumerable from Domain entity without manual string manipulation
        claims.AddRange(token.ScopeList.Select(scope => new Claim("scope", scope)));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return AuthenticateResult.Success(ticket);
    }
}
