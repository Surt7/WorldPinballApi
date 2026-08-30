using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorldPinball.Application.Abstractions.Repositories;
using WorldPinball.Application.Features.Leaderboard.Public.Handlers;
using WorldPinball.Application.Security;
using WorldPinball.Infrastructure.Persistence;
using WorldPinball.Infrastructure.Persistence.Repositories;
using WorldPinball.Infrastructure.Security;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WorldPinball.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
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

        services.AddScoped<GetLeaderboardHandler>();

        return services;
    }
}
