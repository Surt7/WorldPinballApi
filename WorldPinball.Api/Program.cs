using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WorldPinball.Api.Extensions;
using WorldPinball.Api.Logging;
using WorldPinball.Infrastructure.Security;
using WorldPinball.Infrastructure;
using FluentValidation;

using WorldPinball.Api.Middleware;

using WorldPinball.Application.Features.GameSessions.Public.Start;
using WorldPinball.Application.Features.GameSessions.Public.SubmitScore;
using WorldPinball.Application.Features.Leaderboard.Public.Handlers;
using WorldPinball.Application.Features.Machines.Admin.Provision;
using WorldPinball.Application.Features.Machines.Public;
using WorldPinball.Application.Features.Machines.Public.Enroll.Profile;
using WorldPinball.Application.Security.Cryptography;
using WorldPinball.Infrastructure.Security.Cryptography;


// -------------------- BUILDER -------------------- BUILDER -------------------- BUILDER --------------------//

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.AddServerHeader = false;
    });

    builder.Services.AddProblemDetails();

    builder.AddWorldPinballLogging();

    var keycloakAuthority = builder.Configuration["Keycloak:Authority"]
                            ?? throw new InvalidOperationException("Keycloak:Authority is missing.");
    var keycloakAudience = builder.Configuration["Keycloak:Audience"]
                           ?? throw new InvalidOperationException("Keycloak:Audience is missing.");


    builder.Services.AddAutoMapper(cfg => { cfg.AddMaps(typeof(EnrollmentMappingProfile).Assembly); });

// -------------------- Services --------------------
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
    builder.Services.AddWorldPinballLocalization();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.AddObservability();
// ----------------------------------------

    //Handlers
    builder.Services.AddScoped<IEnrollDeviceHandler,
        EnrollDeviceHandler>();
    builder.Services.AddScoped<IProvisionMachineHandler,
        ProvisionMachineHandler>();

    builder.Services.AddScoped<IStartGameSessionHandler,
        StartGameSessionHandler>();
    builder.Services.AddScoped<ISubmitScoreHandler,
        SubmitScoreHandler>();

    builder.Services.AddScoped<IMachineAuthService, MachineAuthService>();
    builder.Services.AddScoped<ICanonicalBuilder, CanonicalBuilder>();

    builder.Services.AddSingleton<ISignatureVerifier,
        EcdsaP256SignatureVerifier>();
    builder.Services.AddScoped<GetLeaderboardHandler>();

    //Validators
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    
// -------------------- AuthN & Z --------------------

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = keycloakAuthority;
            options.Audience = keycloakAudience;
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                NameClaimType = "preferred_username",
                RoleClaimType = "roles",
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

    builder.Services.AddAuthorization();

// -------------------- Open Api Doc --------------------
    builder.Services.AddWorldPinballOpenApi();

// -------------------- APP -------------------- APP -------------------- APP --------------------//
    var app = builder.Build();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment() || app.Environment.IsStaging() )
    {
        await app.Services.MigrateDatabaseAsync();
        await app.Services.SeedDevelopmentDataAsync();

        app.UseDeveloperExceptionPage();
        app.UseWorldPinballApiDocumentation();


        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(
                "/openapi/v1.json",
                "WorldPinball API v1");

            options.OAuthClientId("worldpinball-swagger");
            options.OAuthScopes(
                "openid",
                "profile",
                "email");

            options.OAuthUsePkce();
        });
    }

// -------------------- Healthcheck --------------------
    app.MapApiHealthChecks();


// -------------------- USE -------------------- USE -------------------- USE --------------------//


    app.UseMiddleware<RequireHttpsMiddleware>();


    app.UseRequestLocalization();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();


    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}

public partial class Program
{
}