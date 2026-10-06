using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using WorldPinball.Api.Extensions;
using WorldPinball.Api.Security.Permissions;

namespace WorldPinball.Tests.Api.Security;

public class RequirePermissionPolicyTests
{
    private readonly IAuthorizationService _authService;

    public RequirePermissionPolicyTests()
    {
        var services = new ServiceCollection();
        var config = Substitute.For<IConfiguration>();
        config["Keycloak:Authority"].Returns("http://localhost:8081");
        config["Keycloak:Audience"].Returns("api");
        var env = Substitute.For<IHostEnvironment>();
        env.EnvironmentName.Returns("Development");

        services.AddLogging();
        services.AddApiSecurity(config, env);

        var sp = services.BuildServiceProvider();
        _authService = sp.GetRequiredService<IAuthorizationService>();
    }

    [Fact]
    public async Task EvaluatePolicy_WhenUserIsAdmin_ShouldSucceed()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.Role, "Admin")
        ], "TestAuth", ClaimTypes.Name, ClaimTypes.Role));

        // Act
        var result = await _authService.AuthorizeAsync(user, null, Permission.MachinesProvision.ToString());

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluatePolicy_WhenUserHasPermissionClaim_ShouldSucceed()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("permission", Permission.ShipmentsDispatch.ToString())
        ], "TestAuth"));

        // Act
        var result = await _authService.AuthorizeAsync(user, null, Permission.ShipmentsDispatch.ToString());

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluatePolicy_WhenUserHasNoMatchingPermissionNorAdmin_ShouldFail()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.Role, "Player")
        ], "TestAuth", ClaimTypes.Name, ClaimTypes.Role));

        // Act
        var result = await _authService.AuthorizeAsync(user, null, Permission.ShipmentsDispatch.ToString());

        // Assert
        result.Succeeded.Should().BeFalse();
    }
}
