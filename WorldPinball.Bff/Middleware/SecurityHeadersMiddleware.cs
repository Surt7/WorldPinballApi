namespace WorldPinball.Bff.Middleware;

public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;
    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        var userAgent = context.Request.Headers.UserAgent.ToString();
        
 
     
        
        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers.ContentSecurityPolicy = "default-src 'self';" +
                                        " frame-ancestors 'none';";
        
        headers["Refferer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "accelerometer=(), camera=(), geolocation=(), microphone=()";
        
        using (_logger.BeginScope(new Dictionary<string, object> { ["UserAgent"] = userAgent }))
        {
            await _next(context);
        }
    }
}