using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WorldPinball.Api.Middleware;
using WorldPinball.Infrastructure.Security;


namespace WorldPinball.Tests.Middlewares;

public class CorrelationContextLoggingMiddlewareTests
{
    
    [Fact]
    public async Task InvokeAsync_WhenRequestArrives_ShouldOpenLoggingScopeAndCallNext()
    {
        var next = Substitute.For<RequestDelegate>();
        var logger = Substitute.For<ILogger<CorrelationContextLoggingMiddleware>>();
        var sut = new CorrelationContextLoggingMiddleware(next, logger);
        var httpContext = new DefaultHttpContext();
        await sut.InvokeAsync(httpContext);
        logger.Received(1).BeginScope(Arg.Any<Dictionary<string, object>>());
        await next.Received(1).Invoke(httpContext);
    }


    [Fact]
    public async Task InvokeAsync_WhenSessionHeaderPresent_ShouldIncludeMaskedSessionInScope()
    {
        var next = Substitute.For<RequestDelegate>();
        var logger = Substitute.For<ILogger<CorrelationContextLoggingMiddleware>>();
        var sut = new CorrelationContextLoggingMiddleware(next, logger);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Session-Id"] = "session-guid-123";
        await sut.InvokeAsync(httpContext);
        logger.Received(1).BeginScope(Arg.Is<Dictionary<string, object>>(d =>
            d.ContainsKey("GameSessionId") && d["GameSessionId"].ToString() == "session-guid-123"
        ));
    }
    
    [Fact]
    public async Task InvokeAsync_WhenSessionIdInRouteValues_ShouldIncludeMaskedSessionInScope()
    {
        var next = Substitute.For<RequestDelegate>();
        var logger = Substitute.For<ILogger<CorrelationContextLoggingMiddleware>>();
        var sut = new CorrelationContextLoggingMiddleware(next, logger);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["sessionId"] = "route-guid-456";
        await sut.InvokeAsync(httpContext);
        logger.Received(1).BeginScope(Arg.Is<Dictionary<string, object>>(d =>
            d.ContainsKey("GameSessionId") && d["GameSessionId"].ToString() == "route-guid-456"
        ));
    }
}