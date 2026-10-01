
using Serilog;
using WorldPinball.Api.Extensions;
using WorldPinball.Api.Logging;
using WorldPinball.Infrastructure.Security;
using WorldPinball.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using WorldPinball.Api.Middleware;
using WorldPinball.Application.Features.GameSessions.Public.AddPlayer;
using WorldPinball.Application.Features.GameSessions.Public.CreateGameSession;
using WorldPinball.Application.Features.GameSessions.Public.SubmitScore;
using WorldPinball.Application.Features.Machines.Admin.GetAllMachines;
using WorldPinball.Application.Features.Machines.Admin.Provision;
using WorldPinball.Application.Features.Machines.Admin.ReissueToken;
using WorldPinball.Application.Features.Machines.Public;
using WorldPinball.Application.Features.Machines.Public.Enroll.Profile;
using WorldPinball.Application.Security.Cryptography;
using WorldPinball.Infrastructure.Security.Cryptography;
using WorldPinball.Api.Grpc;
using WorldPinball.Application.Features.GameSessions.Public.GetLobby;
using WorldPinball.Application.Features.GameSessions.Public.KickPlayer;
using WorldPinball.Application.Features.GameSessions.Public.LaunchSession;
using WorldPinball.Application.Features.Leaderboard.Admin.Dashboard;
using WorldPinball.Application.Features.Leaderboard.Admin.Dashboard.Queries;
using WorldPinball.Application.Features.Machines.Admin.CreateOperatorToken;
using WorldPinball.Application.Features.GameModes.Public.GetAllGameModes;
using WorldPinball.Application.Features.GameSessions.Public.AbortSession;
using WorldPinball.Application.Features.GameSessions.Public.RecordBallScore;
using WorldPinball.Application.Features.IpLicenses.Public.GetAllIpLicenses;
using WorldPinball.Application.Features.Leaderboard.Public;
using WorldPinball.Application.Features.Matchmaking.Public.JoinQueue;
using WorldPinball.Application.Features.Matchmaking.Public.LeaveQueue;
using WorldPinball.Application.Features.Matchmaking.Public.MatchNextPlayer;
using WorldPinball.Application.Features.StreamingAccess.Admin;
using WorldPinball.Application.Features.StreamingAccess.Admin.CreateStreamAccessGrant;
using WorldPinball.Application.Features.StreamingAccess.Admin.GetStreamAccessGrants;
using WorldPinball.Application.Features.StreamingAccess.Admin.RevokeStreamAccessGrant;
using WorldPinball.Application.Features.StreamingAccess.Public;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Infrastructure.Services;


// -------------------- BUILDER -------------------- BUILDER -------------------- BUILDER --------------------//

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.AddServerHeader = false;
        options.Limits.Http2.KeepAlivePingDelay = TimeSpan.FromSeconds(60);
        options.Limits.Http2.KeepAlivePingTimeout = TimeSpan.FromSeconds(30);

        options.ConfigureHttpsDefaults(httpsOptions =>
        {
            httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
            httpsOptions.ClientCertificateValidation =
                (certificate, chain, sslPolicyErrors) => true;
        });
    });

    builder.Services.AddProblemDetails();

    builder.AddWorldPinballLogging();
    
    builder.Services.AddRequestTimeouts(options => {
        options.DefaultPolicy = new RequestTimeoutPolicy { 
            Timeout = TimeSpan.FromSeconds(5) 
        };
    });
    builder.Services.AddAutoMapper(cfg => { cfg.AddMaps(typeof(EnrollmentMappingProfile).Assembly); });

// -------------------- Services --------------------
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
    builder.Services.AddWorldPinballLocalization();
    
    
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    
    // Grpc
    builder.Services.AddGrpc();
    builder.Services.AddScoped<IAddPlayerHandler, AddPlayerHandler>();
    builder.Services.AddScoped<IKickPlayerHandler, KickPlayerHandler>();
    builder.Services.AddScoped<IGetLobbyHandler, GetLobbyHandler>();
    builder.Services.AddScoped<ILaunchSessionHandler, LaunchSessionHandler>();
    

    //Handlers
    builder.Services.AddScoped<IEnrollDeviceHandler,
        EnrollDeviceHandler>();
    builder.Services.AddScoped<IProvisionMachineHandler,
        ProvisionMachineHandler>();
    builder.Services.AddScoped<IReissueEnrollmentTokenHandler,
        ReissueEnrollmentTokenHandler>();

    builder.Services.AddScoped<ICreateGameSessionHandler,
        CreateGameSessionHandler>();
    builder.Services.AddScoped<ISubmitScoreHandler,
        SubmitScoreHandler>();
    
    builder.Services.AddScoped<ICreateOperatorTokenHandler,
        CreateOperatorTokenHandler>();
    
    builder.Services.AddScoped<IAbortSessionHandler, AbortSessionHandler>();
    
    builder.Services.AddScoped<IGetMachineDashboardHandler, GetMachineDashboardHandler>();
    builder.Services.AddScoped<IGetAllMachinesHandler, GetAllMachinesHandler>();
    builder.Services.AddScoped<IGetAllGameModesHandler, GetAllGameModesHandler>();
    builder.Services.AddScoped<IGetAllIpLicensesHandler, GetAllIpLicensesHandler>();
    builder.Services.AddScoped<IJoinMatchmakingQueueHandler, JoinMatchmakingQueueHandler>();
    builder.Services.AddScoped<ILeaveMatchmakingQueueHandler, LeaveMatchmakingQueueHandler>();
    builder.Services.AddScoped<IMatchNextPlayerHandler, MatchNextPlayerHandler>();
    builder.Services.AddScoped<IRecordBallScoreHandler, RecordBallScoreHandler>();
    
    builder.Services.AddScoped<IRequestStreamingAccessHandler, RequestStreamingAccessHandler>();
    builder.Services.AddScoped<IApproveStreamingAccessHandler, ApproveStreamingAccessHandler>();
    builder.Services.AddScoped<IGetStreamAccessGrantsHandler, GetStreamAccessGrantsHandler>();
    builder.Services.AddScoped<IRevokeStreamAccessGrantHandler, RevokeStreamAccessGrantHandler>();
    builder.Services.AddScoped<ICreateStreamAccessGrantHandler, CreateStreamAccessGrantHandler>();
    
    
    // Security
    builder.Services.AddScoped<IMachineAuthService, MachineAuthService>();
    builder.Services.AddScoped<ICanonicalBuilder, CanonicalBuilder>();

    builder.Services.AddSingleton<ISignatureVerifier,
        EcdsaP256SignatureVerifier>();
    builder.Services.AddScoped<GetLeaderboardHandler>();

    //Validators
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    
    //SignalR
    builder.Services.AddSignalR();
    builder.Services.AddSingleton<ILobbyNotificationService, InMemoryLobbyNotificationService>();;
    
// -------------------- AuthN & Z --------------------

    builder.Services.AddApiSecurity(builder.Configuration, builder.Environment);

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
    app.UseRequestTimeouts();

    app.UseRequestLocalization();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapGrpcService<GameSessionGrpcService>();

   
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}

namespace WorldPinball.Api
{
    public partial class Program { }
}