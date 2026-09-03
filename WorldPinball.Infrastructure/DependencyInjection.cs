using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Features.Leaderboard.Public.Handlers;
using WorldPinball.Application.Features.Machines.Admin;
using WorldPinball.Application.Security;
using WorldPinball.Infrastructure.Persistence;
using WorldPinball.Infrastructure.Persistence.Repositories;
using WorldPinball.Infrastructure.Persistence.Seed;
using WorldPinball.Infrastructure.Security;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorldPinball.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        

        // -------------------- Time Provider --------------------
        services.AddSingleton(TimeProvider.System);
        
        
        var cs = config.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException(
                     "Connection string 'DefaultConnection' is missing.");

        // -------------------- Database --------------------

        services.AddDbContext<AppDbContext>(options =>
            options
                .UseNpgsql(cs)
                .UseSnakeCaseNamingConvention());

        // -------------------- Database HealthCheck --------------------

        services
            .AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(
                name: "Database",
                tags: ["database"]);
        services.AddScoped<ISecretService, SecretService>();
        services.AddScoped<IIpLicenseRepository, IpLicenseRepository>();
        services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();
        services.AddScoped<IMachineReadRepository, MachineReadRepository>();
        services.AddScoped<IMachineWriteRepository, MachineWriteRepository>();

        services.AddScoped<GetLeaderboardHandler>();
        services.AddScoped<
            IProvisionMachineHandler,
            ProvisionMachineHandler>();

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
