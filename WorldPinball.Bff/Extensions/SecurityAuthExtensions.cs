namespace WorldPinball.Bff.Extensions;

using Azure.Identity;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using StackExchange.Redis;

public static class SecurityAuthExtensions
{
    public static IServiceCollection AddBffSecurity(
        this IServiceCollection services,
        IConfiguration config,
        IHostEnvironment env 
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
                policy.WithOrigins(
                        "https://localhost:4200",
                        "https://worldpinball.local"
                        )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials(); // Mandatory for session and antiforgery cookies
            });
        });
        
        var dataProtection = services.AddDataProtection()
            .SetApplicationName("worldpinball");
        
        if (env.IsDevelopment())
        {
            dataProtection.PersistKeysToFileSystem(new DirectoryInfo(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".worldpinball-keys")));
        }
        else
        {
            var redisConnectionString = config.GetConnectionString("Redis")
                ?? config["Redis:ConnectionString"];

            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                var redis = ConnectionMultiplexer.Connect(redisConnectionString);
                dataProtection.PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
            }

            var keyVaultUri = config["KeyVault:VaultUri"] ?? config["KEYVAULT_URI"];
            
            if (!string.IsNullOrWhiteSpace(keyVaultUri))
            {
                var keyUri = new Uri(new Uri(keyVaultUri), "keys/dataprotection-key");
                dataProtection.ProtectKeysWithAzureKeyVault(
                    keyUri,
                    new DefaultAzureCredential());
            }
        }

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
                options.Cookie.SameSite = SameSiteMode.Lax; // Required for OIDC redirect flows
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.Cookie.IsEssential = true;
                
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                // the pkce challenge is enough to protect the url
                options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;
                options.Authority = config["Keycloak:Authority"] ?? "http://localhost:8081/realms/worldpinball";
                options.ClientId = config["Keycloak:ClientId"] ?? "worldpinball-swagger";
                options.ClientSecret = config["Keycloak:ClientSecret"];
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.RequireHttpsMetadata = false; // local dev
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