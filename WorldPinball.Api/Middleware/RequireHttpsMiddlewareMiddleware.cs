using WorldPinball.Api.Errors;

public sealed class RequireHttpsMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IProblemDetailsService problemDetailsService)
    {
        if (!context.Request.IsHttps)
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            await problemDetailsService.WriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails =
                        new CustomHttpProblemDetails(context)
                });

            return;
        }

        await next(context);
    }
}