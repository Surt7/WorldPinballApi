using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace WorldPinball.Bff.Extensions;

public static class ObservabilityExtensions
{
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("WorldPinball.Bff"))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(
                        options =>
                        {
                            options.Endpoint = new Uri("http://localhost:4318/v1/traces");
                            options.Protocol = OtlpExportProtocol.HttpProtobuf;
                        });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri("http://localhost:4318/v1/metrics");
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    });
            })
            .WithLogging(logging =>
            {
                logging.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri("http://localhost:4318/v1/logs");
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                });
            });;
        return builder;
    }
}