using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

namespace WorldPinball.Api.OpenApi;

public static class OpenApiExtensions
{
    private const string OAuth2Scheme = "oauth2";

    public static IServiceCollection AddWorldPinballOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                ConfigureDocument(document);
                ConfigureOAuth2(document, context.ApplicationServices.GetRequiredService<IConfiguration>());
                AddHealthCheckDocumentation(document);

                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                var endpointMetadata = context.Description.ActionDescriptor.EndpointMetadata;
                var requiresAuthentication = endpointMetadata.OfType<IAuthorizeData>().Any();
                var allowsAnonymous = endpointMetadata.OfType<IAllowAnonymous>().Any();

                if (requiresAuthentication && !allowsAnonymous)
                {
                    operation.Security =
                    [
                        new OpenApiSecurityRequirement
                        {
                            [new OpenApiSecuritySchemeReference(OAuth2Scheme, context.Document, null)] =
                                ["openid", "profile", "email"]
                        }
                    ];
                }

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

    private static void ConfigureOAuth2(OpenApiDocument document, IConfiguration configuration)
    {
        var authority = configuration["Keycloak:Authority"]
            ?? throw new InvalidOperationException("Keycloak:Authority is missing.");

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[OAuth2Scheme] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "Keycloak Authorization Code flow with PKCE.",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{authority}/protocol/openid-connect/auth"),
                    TokenUrl = new Uri($"{authority}/protocol/openid-connect/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        ["openid"] = "OpenID Connect",
                        ["profile"] = "User profile",
                        ["email"] = "Email address"
                    }
                }
            }
        };
    }

    private static void AddHealthCheckDocumentation(OpenApiDocument document)
    {
        document.Paths["/api/health"] = new OpenApiPathItem
        {
            Operations = new Dictionary<System.Net.Http.HttpMethod, OpenApiOperation>
            {
                [System.Net.Http.HttpMethod.Get] = new OpenApiOperation
                {
                    Summary = "Health Check",
                    Description = "Performs a health check on the application",
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse { Description = "OK" },
                        ["503"] = new OpenApiResponse { Description = "Service Unavailable" }
                    }
                }
            }
        };
    }

    public static WebApplication UseWorldPinballApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi();

        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "swagger";
            options.SwaggerEndpoint("../openapi/v1.json", "WorldPinball API v1");
            options.DocumentTitle = "WorldPinball API";
            options.DisplayRequestDuration();
            options.EnableTryItOutByDefault();
            options.OAuthClientId("worldpinball-swagger");
            options.OAuthUsePkce();
            options.OAuthScopes("openid", "profile", "email");
        });

        return app;
    }
}
