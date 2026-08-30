using Microsoft.OpenApi;

namespace WorldPinball.Api.OpenApi;

public static class OpenApiExtensions
{
    public static IServiceCollection AddWorldPinballOpenApi(
        this IServiceCollection services)
    {
        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer(
                (document, context, cancellationToken) =>
                {
                    ConfigureDocument(document);
                    AddHealthCheckDocumentation(document);

                    return Task.CompletedTask;
                });
        });

        return services;
    }

    private static void ConfigureDocument(OpenApiDocument document)
    {
        document.Info = new OpenApiInfo
        {
            Title = "WorldPinball API",
            Version = "v1",
            Description = "WorldPinball leaderboard and machine API"
        };
    }

    private static void AddHealthCheckDocumentation(
        OpenApiDocument document)
    {
        document.Paths["/api/health"] = new OpenApiPathItem
        {
            Operations =
                new Dictionary<System.Net.Http.HttpMethod, OpenApiOperation>
                {
                    [System.Net.Http.HttpMethod.Get] = new OpenApiOperation
                    {
                        Summary = "Health Check",
                        Description =
                            "Performs a health check on the application",

                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Description = "OK"
                            },

                            ["503"] = new OpenApiResponse
                            {
                                Description = "Service Unavailable"
                            }
                        }
                    }
                }
        };
    }
}