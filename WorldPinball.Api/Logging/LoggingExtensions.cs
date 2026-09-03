using Serilog;
using Serilog.Events;

namespace WorldPinball.Api.Logging;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddWorldPinballLogging(
        this WebApplicationBuilder builder)
    {
        var seqServerUrl = builder.Configuration["Seq:ServerUrl"];

        builder.Services.AddSerilog((services, configuration) =>
        {
            configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override(
                    "Microsoft.AspNetCore",
                    LogEventLevel.Warning)
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "WorldPinball.Api")
                .WriteTo.Console();

            if (!string.IsNullOrWhiteSpace(seqServerUrl))
            {
                configuration.WriteTo.Seq(seqServerUrl);
            }
        });

        return builder;
    }
}
