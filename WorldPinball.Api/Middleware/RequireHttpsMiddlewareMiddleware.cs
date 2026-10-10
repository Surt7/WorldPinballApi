using WorldPinball.Api.Errors;
namespace WorldPinball.Api.Middleware;
public sealed class RequireHttpsMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IProblemDetailsService problemDetailsService,
            IHostEnvironment env
        )
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
                        new HttpsRequiredProblemDetails(context, env)
                });

            return;
        }

        await next(context);
    }
}