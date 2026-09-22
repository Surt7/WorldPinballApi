using Microsoft.AspNetCore.Http.Timeouts;
using WorldPinball.Bff.Extensions;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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
builder.Services.AddBffSecurity(); 
builder.Services.AddBffRateLimiting();
builder.Services.AddHealthChecks();

builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler();
});

builder.AddObservability();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseSecurityHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseRequestTimeouts();
app.UseHttpsRedirection();
app.UseCors("AngularDevPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Health check endpoint for Docker / K8s
app.MapHealthChecks("/health");

app.MapReverseProxy().RequireRateLimiting("public-ip-limit");

app.Run();