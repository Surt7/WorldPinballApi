using Microsoft.AspNetCore.Mvc;

namespace WorldPinball.Api.Filters.Idempotency;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class IdempotentAttribute : TypeFilterAttribute
{
    public IdempotentAttribute() : base(typeof(IdempotencyFilter)) { }

    public string HeaderName { get; init; } = "X-Idempotency-Key";
    public int ExpirationMinutes { get; init; } = 60;
    public bool RequireHeader { get; init; } = false;
}