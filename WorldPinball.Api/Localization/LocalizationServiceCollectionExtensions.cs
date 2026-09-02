using Microsoft.AspNetCore.Builder;

namespace WorldPinball.Api.Localization;

public static class LocalizationServiceCollectionExtensions
{

    public static IServiceCollection AddWorldPinballLocalization(this IServiceCollection services)
    {
        services.AddLocalization(
            options => options.ResourcesPath = "Resources");

        string[] supportedCultures = ["en", "fr"];

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options
                .SetDefaultCulture("en")
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
        });
        
        return services;
    }
    
}