using Microsoft.AspNetCore.Mvc;

namespace WorldPinball.Api.Errors;

public class CustomHttpProblemDetails : ProblemDetails
{
    public CustomHttpProblemDetails(HttpContext context, IHostEnvironment env)
    {
        Title = "Bad Request";
        Status = StatusCodes.Status400BadRequest;
        Detail = "HTTP requests are not allowed. Please use HTTPS.";

        Instance =
            $"{context.Request.Path} ({context.TraceIdentifier})";

        
        
        /* We send this only in dev only for two main reasons:
        *This header is sensitive as it contains informatios for proxies and private ip...
        *And Dictionnary is sensitive to DDOS attack (as one form is hashcode collision attack)
        */ 
        if (env.IsDevelopment())
        {
            Extensions["headers"] = new Dictionary<string, string?>
            {
                ["Host"] = context.Request.Headers.Host,
                ["User-Agent"] = context.Request.Headers.UserAgent,
                ["X-Forwarded-Proto"] = context.Request.Headers["X-Forwarded-Proto"],
                ["X-Forwarded-For"] = context.Request.Headers["X-Forwarded-For"]
            };
        }
    }
}