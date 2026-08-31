using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WorldPinball.Domain.Errors;

namespace WorldPinball.Api.Errors;

public sealed class DomainExceptionHandler(IStringLocalizer<SharedResource> localizer)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not IDomainException domainException)
        {
            return false;
        }

        var statusCode = exception is DomainValidationException
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status409Conflict;

        var localizedDetail = localizer[domainException.ErrorCode];
        var detail = localizedDetail.ResourceNotFound
            ? exception.Message
            : localizedDetail.Value;

        var titleKey = statusCode == StatusCodes.Status400BadRequest
            ? "Errors.ValidationTitle"
            : "Errors.DomainRuleTitle";
        var localizedTitle = localizer[titleKey];

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = localizedTitle.ResourceNotFound ? titleKey : localizedTitle.Value,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["code"] = domainException.ErrorCode;

        if (exception is ArgumentException argumentException)
        {
            problem.Extensions["parameter"] = argumentException.ParamName;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
