using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Timeouts;
using WorldPinball.Bff.Extensions;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WorldPinball.Bff.Endpoints;
using WorldPinball.Bff.Hubs;
using WorldPinball.Bff.Services;
using WorldPinball.Protos.V1.GameSessions;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});
// I Configured cancellation token source 5 sec timeout
builder.Services.AddRequestTimeouts(options => {
    options.DefaultPolicy = new RequestTimeoutPolicy { 
        Timeout = TimeSpan.FromSeconds(5) 
    };
});

/*These two references help me to configure security
 *ASP.NET Core Security by Christian Wenz Manning edition
 *https://cheatsheetseries.owasp.org/cheatsheets/HTTP_Headers_Cheat_Sheet.html
 */

builder.Services.AddBffSecurity(builder.Configuration); 
builder.Services.AddBffRateLimiting();
builder.Services.AddHealthChecks()
    .AddCheck("self", 
        () => Microsoft
            .Extensions
            .Diagnostics
            .HealthChecks
            .HealthCheckResult
            .Healthy(), 
        ["live"]);

builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler();
});

builder.AddObservability();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(async transformContext =>
        {
            
            // we have to inject the JWT with YARP
            var accessToken = await transformContext.HttpContext.GetTokenAsync("access_token");
            if (!string.IsNullOrEmpty(accessToken))
            {
                transformContext.ProxyRequest.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }
        });
    });
    ;

    builder.Services.AddSignalR();
    builder.Services.AddSingleton<ILobbyRelayService, LobbyRelayService>();
    builder.Services.AddGrpcClient<GameSessionService.GameSessionServiceClient>(options =>
    {
        var coreApiUrl = builder.Configuration["Services:CoreApi:Https"] 
                         ?? builder.Configuration["ReverseProxy:Clusters:core-api-cluster:Destinations:destination1:Address"]
                         ?? "https://localhost:7010";
        options.Address = new Uri(coreApiUrl);
    });
// -------------------- RESOLVE APP USE --------------------
    
var app = builder.Build();

app.UseSecurityHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseRequestTimeouts();
app.UseHttpsRedirection();
app.UseCors("AngularDevPolicy");
app.MapHealthChecks("/alive", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live")
});

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Health check endpoint for Docker / K8s

app.MapHealthChecks("/health");
app.MapAuthEndpoints();
app.MapHub<LobbyHub>("/hubs/lobby");
app.MapReverseProxy().RequireRateLimiting("public-ip-limit");

app.Run();

// Because it can raise ambiguity errors
namespace WorldPinball.Bff
{ 
    public partial class Program { }
}