using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using NSubstitute;
using WorldPinball.Api.Contracts.Machine;
using WorldPinball.Application.Contracts.Admin.Machine;
using WorldPinball.Application.Features.Machines.Admin;
using WorldPinball.Application.Features.Machines.Admin.Provision;
using WorldPinball.Application.Features.Machines.Admin.Provision.Commands;

namespace WorldPinball.Tests.Integration;

public class AdminMachineAuthorizationTests : IClassFixture<WorldPinballApiFactory>
{
    private readonly HttpClient _client;
    // Keep the same factory instance that owns the handler substitute registered in the test server.
    private readonly WorldPinballApiFactory _factory;
    
    public AdminMachineAuthorizationTests(WorldPinballApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                // The production middleware rejects requests that are not marked as HTTPS.
                BaseAddress = new Uri("https://localhost")
            }
        );
    }

    // No test identity headers: the test authentication handler returns NoResult, leading to 401.
    [Theory]
    [InlineData("/api/v1/admin/machines/provision")]
    public async Task Provision_WhenUnauthenticated_ShouldReturnUnauthorized(string url)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            url);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    // An authenticated non-admin user must be rejected by the real ASP.NET Core authorization layer.
    [Theory]
    [InlineData("/api/v1/admin/machines/provision")]
    public async Task Provision_WhenAuthenticatedButNotAdmin_ShouldReturnForbidden(string url)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            url);

        request.Headers.Add(
            TestAuthHandler.UserHeaderName,
            "test-user");
        request.Headers.Add(
            TestAuthHandler.RoleHeaderName,
            "Player");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("/api/v1/admin/machines/provision")]
    public async Task Provision_WhenAuthenticatedAndAdmin_ShouldReturnAuthorized(string url)
    {
        // Avoid database work while letting the real controller complete its successful HTTP path.
        var expectedMachineId = Guid.NewGuid();
        const string expectedEnrollmentToken = "integration-enrollment-token";
        var idempotencyKey = Guid.NewGuid();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            url);
        _factory.ProvisionMachineHandler
            .HandleAsync(
                Arg.Any<ProvisionMachineCommand>(),
                Arg.Any<CancellationToken>())
            .Returns(
                new ProvisionMachineResult.Success(
                    expectedMachineId,
                    expectedEnrollmentToken));
        request.Headers.Add(
            TestAuthHandler.UserHeaderName,
            "test-user");
        request.Headers.Add(
            TestAuthHandler.RoleHeaderName,
            "Admin");

        // Send a real JSON body so MVC model binding and request validation remain part of the test.
        request.Content = JsonContent.Create(
            new ProvisionMachineRequest(
                "WP-INTEGRATION-001",
                "resident-evil",
                idempotencyKey,
                "Integration machine",
                null));
        var response = await _client.SendAsync(request);

        response.StatusCode.Should()
            .Be(HttpStatusCode.OK);
    }

}
