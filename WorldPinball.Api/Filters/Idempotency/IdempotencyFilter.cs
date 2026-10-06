using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WorldPinball.Api.Filters.Idempotency;

public sealed class IdempotencyFilter(
    IIdempotencyStore store,
    ILogger<IdempotencyFilter> logger
) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var cad = context.ActionDescriptor as ControllerActionDescriptor;
        var attribute = cad?.MethodInfo.GetCustomAttribute<IdempotentAttribute>()
                        ?? cad?.ControllerTypeInfo.GetCustomAttribute<IdempotentAttribute>();

        if (attribute is null)
        {
            await next();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(attribute.HeaderName, out var key)
            || string.IsNullOrWhiteSpace(key))
        {
            if (attribute.RequireHeader)
            {
                context.Result = new BadRequestObjectResult(
                    new { error = $"Header '{attribute.HeaderName}' is required for this operation." });
                return;
            }

            await next();
            return;
        }

        var cached = await store.GetAsync(key!, context.HttpContext.RequestAborted);
        if (cached is not null)
        {
            logger.LogInformation("Idempotent response served for key {Key}", (string)key!);
            context.Result = new ContentResult
            {
                StatusCode = cached.StatusCode,
                ContentType = cached.ContentType,
                Content = cached.Body
            };
            return;
        }

        var executedContext = await next();

        if (executedContext.Result is ObjectResult objectResult && objectResult.StatusCode is >= 200 and < 300)
        {
            var body = JsonSerializer.Serialize(objectResult.Value);
            var responseToCache = new IdempotentResponse(
                objectResult.StatusCode ?? 200,
                "application/json",
                body);

            await store.SetAsync(
                key!,
                responseToCache,
                TimeSpan.FromMinutes(attribute.ExpirationMinutes),
                context.HttpContext.RequestAborted);
        }
    }
}