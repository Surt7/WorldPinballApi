using WorldPinball.Bff.Middleware;

namespace WorldPinball.Bff.Extensions;

public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
    
}