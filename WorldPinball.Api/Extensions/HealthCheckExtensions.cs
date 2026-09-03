using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using WorldPinball.Api.Health;

namespace WorldPinball.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddApiHealthChecks(
        this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy(),
                tags: ["live"]);

        return services;
    }

    public static IEndpointRouteBuilder MapApiHealthChecks(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/api/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteAsync,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] =
                    StatusCodes.Status503ServiceUnavailable
            }
        });

        return endpoints;
    }
}
