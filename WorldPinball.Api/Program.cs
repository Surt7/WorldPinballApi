using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WorldPinball.Api.Extensions;
using WorldPinball.Api.Logging;
using WorldPinball.Infrastructure.Security;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Infrastructure;
using WorldPinball.Infrastructure.Persistence.Repositories;
using FluentValidation;
using WorldPinball.Application.Contracts.Admin.Machine;
using WorldPinball.Api.Validation.Admin;

// -------------------- BUILDER -------------------- BUILDER -------------------- BUILDER --------------------//
try
{


    var builder = WebApplication.CreateBuilder(args);
    
    builder.Services.AddProblemDetails();
    
    builder.AddWorldPinballLogging();

    var keycloakAuthority = builder.Configuration["Keycloak:Authority"]
                            ?? throw new InvalidOperationException("Keycloak:Authority is missing.");
    var keycloakAudience = builder.Configuration["Keycloak:Audience"]
                           ?? throw new InvalidOperationException("Keycloak:Audience is missing.");


// -------------------- Services --------------------
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
    builder.Services.AddWorldPinballLocalization();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

// ----------------------------------------
    builder.Services.AddScoped<IMachineAuthService, MachineAuthService>();
    builder.Services.AddScoped<ICanonicalBuilder, CanonicalBuilder>();
    builder.Services.AddSingleton<ISignatureVerifier, EcdsaP256SignatureVerifier>();
    builder.Services.AddScoped<IGameModeRepository, GameModeRepository>();
    builder.Services.AddScoped<IIpLicenseRepository, IpLicenseRepository>();
    builder.Services.AddScoped<IValidator<ProvisionMachineRequest>,
        ProvisionMachineRequestValidator>();

// DataProtection keys folder (dev vs prod)
    var keysDir = builder.Environment.IsDevelopment()
        ? new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "dpkeys"))
        : new DirectoryInfo("/var/lib/worldpinball/dpkeys");

    builder.Services
        .AddDataProtection()
        .SetApplicationName("WorldPinball")
        .PersistKeysToFileSystem(keysDir);
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

    if (app.Environment.IsDevelopment())
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

// -------- Middleware pipeline --------
    app.UseMiddleware<RequireHttpsMiddleware>();
    
    
    app.UseRequestLocalization();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
  

    app.Run();
}
catch(Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}