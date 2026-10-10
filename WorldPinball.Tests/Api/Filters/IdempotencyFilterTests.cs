using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Api.Filters.Idempotency;

namespace WorldPinball.Tests.Filters;

public class IdempotencyFilterTests
{
    private readonly IIdempotencyStore _store = Substitute.For<IIdempotencyStore>();
    private readonly IdempotencyFilter _sut;

    public IdempotencyFilterTests()
    {
        _sut = new IdempotencyFilter(_store, NullLogger<IdempotencyFilter>.Instance);
    }

    private class SampleController
    {
        [Idempotent(RequireHeader = true, ExpirationMinutes = 15)]
        public IActionResult WithRequiredHeader() => new OkObjectResult(new { message = "hello" });

        [Idempotent(RequireHeader = false)]
        public IActionResult WithOptionalHeader() => new OkObjectResult(new { message = "hello" });

        public IActionResult WithoutAttribute() => new OkResult();
    }

    private static (ActionExecutingContext context, ActionExecutionDelegate next, Func<bool> nextCalled) CreateContext(
        string methodName, 
        string? headerValue = null)
    {
        var httpContext = new DefaultHttpContext();
        if (headerValue is not null)
        {
            httpContext.Request.Headers["X-Idempotency-Key"] = headerValue;
        }

        var methodInfo = typeof(SampleController).GetMethod(methodName)!;
        var actionDescriptor = new ControllerActionDescriptor
        {
            MethodInfo = methodInfo,
            ControllerTypeInfo = typeof(SampleController).GetTypeInfo()
        };

        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);
        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>()!,
            new SampleController());

        bool isNextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            isNextCalled = true;
            var executed = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new SampleController())
            {
                Result = new OkObjectResult(new { status = "success" })
            };
            return Task.FromResult(executed);
        };

        return (executingContext, next, () => isNextCalled);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenNoAttribute_ShouldInvokeNext()
    {
        // Arrange
        var (context, next, nextCalled) = CreateContext(nameof(SampleController.WithoutAttribute));

        // Act
        await _sut.OnActionExecutionAsync(context, next);

        // Assert
        nextCalled().Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenHeaderMissingAndRequired_ShouldReturnBadRequest()
    {
        // Arrange
        var (context, next, nextCalled) = CreateContext(nameof(SampleController.WithRequiredHeader), headerValue: null);

        // Act
        await _sut.OnActionExecutionAsync(context, next);

        // Assert
        nextCalled().Should().BeFalse();
        context.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenHeaderMissingAndOptional_ShouldInvokeNext()
    {
        // Arrange
        var (context, next, nextCalled) = CreateContext(nameof(SampleController.WithOptionalHeader), headerValue: null);

        // Act
        await _sut.OnActionExecutionAsync(context, next);

        // Assert
        nextCalled().Should().BeTrue();
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenCached_ShouldReturnCachedResultWithoutCallingNext()
    {
        // Arrange
        var (context, next, nextCalled) = CreateContext(nameof(SampleController.WithRequiredHeader), headerValue: "KEY-123");
        var cached = new IdempotentResponse(200, "application/json", "{\"status\":\"cached\"}");
        _store.GetAsync("KEY-123", Arg.Any<CancellationToken>()).Returns(cached);

        // Act
        await _sut.OnActionExecutionAsync(context, next);

        // Assert
        nextCalled().Should().BeFalse();
        context.Result.Should().BeOfType<ContentResult>()
            .Which.Content.Should().Be("{\"status\":\"cached\"}");
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenNotCached_ShouldCallNextAndStoreResult()
    {
        // Arrange
        var (context, next, nextCalled) = CreateContext(nameof(SampleController.WithRequiredHeader), headerValue: "KEY-456");
        _store.GetAsync("KEY-456", Arg.Any<CancellationToken>()).Returns((IdempotentResponse?)null);

        // Act
        await _sut.OnActionExecutionAsync(context, next);

        // Assert
        nextCalled().Should().BeTrue();
        await _store.Received(1).SetAsync(
            "KEY-456", 
            Arg.Is<IdempotentResponse>(r => r.StatusCode == 200), 
            TimeSpan.FromMinutes(15), 
            Arg.Any<CancellationToken>());
    }
}
