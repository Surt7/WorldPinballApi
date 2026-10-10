using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

using WorldPinball.Application.Features.Machines.Admin.Provision;

namespace WorldPinball.Tests.Integration;

public sealed class WorldPinballApiFactory:WebApplicationFactory<global::WorldPinball.Api.Program>
{
    // Exposed so each test can configure the exact result returned by the instance used by the server.
    public IProvisionMachineHandler ProvisionMachineHandler { get; } =
        Substitute.For<IProvisionMachineHandler>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Prevent development-only database migrations and seed operations during test startup.
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Replace the production handler to keep authorization tests deterministic and database-free.
            services.RemoveAll<IProvisionMachineHandler>();
            services.AddSingleton(ProvisionMachineHandler);

            services.RemoveAll<WorldPinball.Application.Interfaces.Services.IMachineStatusCacheService>();
            var cacheMock = Substitute.For<WorldPinball.Application.Interfaces.Services.IMachineStatusCacheService>();
            services.AddSingleton(cacheMock);

            services.RemoveAll<WorldPinball.Api.Filters.Idempotency.IIdempotencyStore>();
            services.AddSingleton<WorldPinball.Api.Filters.Idempotency.IIdempotencyStore, WorldPinball.Api.Filters.Idempotency.InMemoryIdempotencyStore>();

            // Replace JWT/Keycloak authentication only; ASP.NET Core authorization remains real.
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme =
                        TestAuthHandler.SchemeName;
                    options.DefaultForbidScheme =
                        TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ => { });
        });
    }
}
