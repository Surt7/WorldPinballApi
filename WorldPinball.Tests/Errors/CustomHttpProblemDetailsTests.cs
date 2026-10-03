using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using WorldPinball.Api.Errors;
using Xunit;

namespace WorldPinball.Tests.Errors;

public class CustomHttpProblemDetailsTests
{
    [Fact]
    public void Constructor_WhenInProduction_ShouldNotIncludeHeadersInExtensions()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var env = Substitute.For<IHostEnvironment>();
        env.EnvironmentName.Returns(Environments.Production);

        // Act
        var problemDetails = new CustomHttpProblemDetails(httpContext, env);

        // Assert
        problemDetails.Extensions.Should().NotContainKey("headers");
    }

    [Fact]
    public void Constructor_WhenInDevelopment_ShouldIncludeHeadersInExtensions()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var env = Substitute.For<IHostEnvironment>();
        env.EnvironmentName.Returns(Environments.Development);

        // Act
        var problemDetails = new CustomHttpProblemDetails(httpContext, env);

        // Assert
        problemDetails.Extensions.Should().ContainKey("headers");
    }
}