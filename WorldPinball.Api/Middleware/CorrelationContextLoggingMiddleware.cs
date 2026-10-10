using WorldPinball.Infrastructure.Security;

namespace WorldPinball.Api.Middleware;

public sealed class CorrelationContextLoggingMiddleware(
    RequestDelegate next,
    ILogger<CorrelationContextLoggingMiddleware> logger,
    SensitiveDataMasker masker
    )
{
    public async Task InvokeAsync(HttpContext context)
    {
        var scopeData = new Dictionary<string, object>
        {
            // Object index Initializer for IL
            ["CorrelationId"] = context.TraceIdentifier
        };
        
        string? rawSessionId = null;
        if (context.Request.RouteValues.TryGetValue("sessionId", out var routeVal))
        {
            // Check from Route
            rawSessionId = routeVal?.ToString();
        }
        else if (context.Request.Headers.TryGetValue("X-Session-Id", out var headerVal))
        {
            // Check from Headers
            rawSessionId = headerVal.ToString();
        }

        if (!string.IsNullOrWhiteSpace(rawSessionId))
        {
            // Then we apply the mask 
            scopeData["GameSessionId"] = masker.Mask("GameSessionId", rawSessionId)!;
        }
        
        using (logger.BeginScope(scopeData))
        {
            await next(context);
        }
    }
}