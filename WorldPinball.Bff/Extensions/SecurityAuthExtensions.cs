namespace WorldPinball.Bff.Extensions;

using Microsoft.AspNetCore.Authentication.Cookies;

public static class SecurityAuthExtensions
{
    public static IServiceCollection AddBffSecurity(this IServiceCollection services)
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
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "__Host-worldpinball-session";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.Cookie.IsEssential = true;
            });
        services.AddAuthorization();
      
      
        services.AddAntiforgery(options =>
        {
            // Le header que le client Angular enverra
            options.HeaderName = "X-XSRF-TOKEN";
    
            // Le cookie de protection interne du serveur (HttpOnly + Secure)
            options.Cookie.Name = "__Host-worldpinball-antiforgery";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });
        

        return services;
    }
}