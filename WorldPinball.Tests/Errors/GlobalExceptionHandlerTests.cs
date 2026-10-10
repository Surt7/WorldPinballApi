using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WorldPinball.Api.Errors;

namespace WorldPinball.Tests.Errors;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WhenExceptionOccurs_ShouldWriteProblemDetailsAndReturnTrue()
    {
        // Arrange
        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        var problemDetailsService = Substitute.For<IProblemDetailsService>();
        var sut = new GlobalExceptionHandler(logger, problemDetailsService);
        var httpContext = new DefaultHttpContext();
        var exception = new InvalidOperationException("Database connection timeout");   
        
        
        // Act
        var result = await sut.TryHandleAsync(httpContext, exception, CancellationToken.None);
        
        
        // Assert
        result.Should().BeTrue();
        await problemDetailsService.Received(1).WriteAsync(Arg.Is<ProblemDetailsContext>(c => 
            c.ProblemDetails.Status == StatusCodes.Status500InternalServerError
        ));
    }
}