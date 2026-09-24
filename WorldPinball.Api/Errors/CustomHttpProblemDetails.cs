using Microsoft.AspNetCore.Mvc;

namespace WorldPinball.Api.Errors;

public class CustomHttpProblemDetails : ProblemDetails
{
    public CustomHttpProblemDetails(HttpContext context)
    {
        Title = "Bad Request";
        Status = StatusCodes.Status400BadRequest;
        Detail = "HTTP requests are not allowed. Please use HTTPS.";

        Instance =
            $"{context.Request.Path} ({context.TraceIdentifier})";

        Dictionary<string, string?> additionalHeaders = new()
        {
            ["Host"] = context.Request.Headers["Host"],
            ["User-Agent"] = context.Request.Headers["User-Agent"],
            ["X-Forwarded-Proto"] =
                context.Request.Headers["X-Forwarded-Proto"],
            ["X-Forwarded-For"] =
                context.Request.Headers["X-Forwarded-For"]
        };
         
        // Foward is meant to be useful for proxy
        Extensions["headers"] = additionalHeaders;
    }
}