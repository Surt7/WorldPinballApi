using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using WorldPinball.Api.Health;
using WorldPinball.Api.OpenApi;
using WorldPinball.Infrastructure.Security;
using WorldPinball.Application.Abstractions.Repositories;
using WorldPinball.Infrastructure;
using WorldPinball.Infrastructure.Persistence.Repositories;


var builder = WebApplication.CreateBuilder(args);





// -------------------- Services --------------------
builder.Services.AddInfrastructure(builder.Configuration);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IMachineAuthService, MachineAuthService>();
builder.Services.AddScoped<ICanonicalBuilder, CanonicalBuilder>();
builder.Services.AddSingleton<ISignatureVerifier, EcdsaP256SignatureVerifier>();
builder.Services.AddScoped<IGameModeRepository, GameModeRepository>();
builder.Services.AddScoped<IIpLicenseRepository, IpLicenseRepository>();


// DataProtection keys folder (dev vs prod)
var keysDir = builder.Environment.IsDevelopment()
    ? new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "dpkeys"))
    : new DirectoryInfo("/var/lib/worldpinball/dpkeys");

builder.Services
    .AddDataProtection()
    .SetApplicationName("WorldPinball")
    .PersistKeysToFileSystem(keysDir);

// Authentication is intentionally left unconfigured until Keycloak is connected.
// AddJwtBearer keeps the authentication scheme available for [Authorize] endpoints.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization();

// -------------------- Open Api Doc --------------------
builder.Services.AddWorldPinballOpenApi();


// -------------------- APP --------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}



// -------------------- Healthcheck --------------------

app.MapApiHealthChecks();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
