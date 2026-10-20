using WorldPinball.Infrastructure.Security;

namespace WorldPinball.Api.Middleware;

/// <summary>
/// HTTP middleware that establishes an enriched, ambient logging scope for distributed tracing and observability.
/// Injects the request's <see cref="HttpContext.TraceIdentifier"/> as a CorrelationId so all log entries across
/// downstream services and handlers can be correlated in log aggregators (e.g., Grafana Loki, Elasticsearch).
/// Sanitizes and masks sensitive identifiers (such as session tokens) via <see cref="SensitiveDataMasker"/>
/// before adding them to the logging scope, enforcing security and GDPR compliance.
/// </summary>
public sealed class CorrelationContextLoggingMiddleware(
    RequestDelegate next,
    ILogger<CorrelationContextLoggingMiddleware> logger
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
            scopeData["GameSessionId"] = SensitiveDataMasker.Mask("GameSessionId", rawSessionId)!;
        }
        
        using (logger.BeginScope(scopeData))
        {
            await next(context);
        }
    }
}