namespace WorldPinball.Bff.Extensions;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

public static class SecurityAuthExtensions
{
    public static IServiceCollection AddBffSecurity(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
            options.HttpsPort = 7285;
        });
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(730); // 2 years
        });
        services.AddCors(options =>
        {
            options.AddPolicy("AngularDevPolicy", policy =>
            {
                policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials(); // Mandatory for session and antiforgery cookies
            });
        });
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "__Host-worldpinball-session";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.Cookie.IsEssential = true;
                
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                // the pkce is enough to protect the url
                options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;
                options.Authority = config["Keycloak:Authority"] ?? "http://localhost:8081/realms/worldpinball";
                options.ClientId = config["Keycloak:ClientId"] ?? "worldpinball-swagger";
                options.ClientSecret = config["Keycloak:ClientSecret"];
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.RequireHttpsMetadata = false; // Dev local
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                
                // Query mode with http issue - ephemeral cookie (the pkce protect against that too)
                options.ResponseMode = OpenIdConnectResponseMode.Query;
                options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.NonceCookie.SameSite = SameSiteMode.Unspecified;
                
                
            });
        ;
        services.AddAuthorization();


        services.AddAntiforgery(options =>
        {
            // the header that Angular will send to client
            options.HeaderName = "X-XSRF-TOKEN";

            // protecting cookie (HttpOnly + Secure)
            options.Cookie.Name = "__Host-worldpinball-antiforgery";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });


        return services;
    }
}