using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WorldPinball.Api.Health;

public static class HealthCheckResponseWriter
{
    public static async Task WriteAsync(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),

            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration
            }),

            totalDuration = report.TotalDuration
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}