using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Infrastructure.Persistence;
using WorldPinball.Infrastructure.Persistence.Repositories;
using WorldPinball.Infrastructure.Persistence.Seed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Application.Security.CA;
using WorldPinball.Application.Security.Secrets;
using WorldPinball.Infrastructure.Persistence.Interceptors;
using WorldPinball.Infrastructure.Security.Authentification;
using WorldPinball.Infrastructure.Security.Authorization;
using WorldPinball.Infrastructure.Security.CA;
using WorldPinball.Infrastructure.Security.Secrets;
using WorldPinball.Infrastructure.Services;

namespace WorldPinball.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        

        // -------------------- Time Provider --------------------
        services.AddSingleton(TimeProvider.System);
        
        
        // -------------------- Database + Interceptors--------------------
        
        var cs = config.GetRequiredPostgresConnectionString();

        
        //+ Interceptors
        services.AddSingleton<PublishDomainEventsInterceptor>();
        services.AddDbContext<AppDbContext>((sp,options) =>
            options
                .UseNpgsql(cs)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(
                    sp.GetRequiredService<PublishDomainEventsInterceptor>()
                    ));
        
        // -------------------- Database health check --------------------

        services
            .AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(
                name: "Database",
                tags: ["database"]);
       
        //Repositories
        services.AddScoped<ISecretService, SecretService>();
        services.AddScoped<IIpLicenseReadRepository, IpLicenseReadRepository>();
        services.AddScoped<ILeaderboardReadRepository, LeaderboardReadRepository>();
        services.AddScoped<IMachineReadRepository, MachineReadRepository>();
        services.AddScoped<IMachineWriteRepository, MachineWriteRepository>();
        services.AddScoped<IGameModeRepository, GameModeRepository>();
        services.AddScoped<IIpLicenseReadRepository, IpLicenseReadRepository>();

        services.AddScoped<IDeviceEnrollmentReadRepository, DeviceEnrollmentReadRepository>();
        services.AddScoped<IDeviceEnrollmentWriteRepository, DeviceEnrollmentWriteRepository>();

        services.AddScoped<IMachineReadRepository, MachineReadRepository>();
        services.AddScoped<IMachineWriteRepository, MachineWriteRepository>();

        services.AddScoped<IGameSessionReadRepository, GameSessionReadRepository>();
        services.AddScoped<IGameSessionWriteRepository, GameSessionWriteRepository>();
        services.AddScoped<ICertificateAuthorityService, StepCaCertificateAuthorityService>();
        
       
        
        
        // For Auth policies check
        services.AddTransient<IClaimsTransformation, MachineClaimsTransformation>();
        
        
        // Auth policies
        services.AddSingleton<IAuthorizationHandler, ActiveMachineAuthorizationHandler>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ActiveMachineOnly", policy =>
                policy.Requirements.Add(new ActiveMachineRequirement()));
        });
// -------------------- Redis --------------------
       
        services.AddScoped<IRealtimeLeaderboardService, RedisRealtimeLeaderboardService>();

        var redisHost = config["Redis:Host"] ?? "localhost:6379";
        var redisPassword = config["Redis:Password"];
        
        var configurationOptions = ConfigurationOptions.Parse(redisHost);
        if (!string.IsNullOrWhiteSpace(redisPassword))
        {
            configurationOptions.Password = redisPassword;
        }
        configurationOptions.AbortOnConnectFail = false;
        
        services.AddSingleton<IConnectionMultiplexer>(sp => 
            ConnectionMultiplexer.Connect(configurationOptions));
        
        
        services.AddScoped<IMachineStatusCacheService, RedisMachineStatusCacheService>();
        
        
        return services;
    }

    public static async Task MigrateDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync(cancellationToken);
    }

    public static async Task SeedDevelopmentDataAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        
        var logger =
            loggerFactory.CreateLogger(
                "WorldPinball.DevelopmentDataSeeder");
        await DevelopmentDataSeeder.SeedAsync(
            context,
            timeProvider,
            logger,
            cancellationToken);
    }
}
