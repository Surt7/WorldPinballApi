using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WorldPinball.Infrastructure.Security.Authorization;

namespace WorldPinball.Api.Extensions;

public static class SecurityExtensions
{
    public static IServiceCollection AddApiSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var keycloakAuthority = configuration["Keycloak:Authority"]
                                ?? throw new InvalidOperationException("Keycloak:Authority is missing.");
        var keycloakAudience = configuration["Keycloak:Audience"]
                               ?? throw new InvalidOperationException("Keycloak:Audience is missing.");


        // 1. Authentication (JWT Bearer for Web/Admin + mTLS Client Certificate for IoT Machines)
        services
            .AddAuthentication(options => { options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
            .AddJwtBearer(options =>
            {
                options.Authority = keycloakAuthority;
                options.Audience = keycloakAudience;
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    NameClaimType = "preferred_username",
                    RoleClaimType = "roles",
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            })
            .AddCertificate(options =>
            {
                options.AllowedCertificateTypes = CertificateTypes.All;
                options.ValidateCertificateUse = false;
                options.ValidateValidityPeriod = false;
                options.RevocationMode = X509RevocationMode.NoCheck;
                options.ChainTrustValidationMode = X509ChainTrustMode.CustomRootTrust;
                var rootCert = new X509Certificate2(File.ReadAllBytes("root_ca.crt"));
                options.CustomTrustStore.Add(rootCert);
                var interCert = new X509Certificate2(File.ReadAllBytes("intermediate_ca.crt"));
                options.CustomTrustStore.Add(interCert);
     
                options.Events = new CertificateAuthenticationEvents
                {
                    OnAuthenticationFailed = async context =>
                    {
                        if (context.HttpContext.Connection.ClientCertificate is not null)
                        {
                            var validatedContext = new CertificateValidatedContext(context.HttpContext, context.Scheme, options)
                            {
                                ClientCertificate = context.HttpContext.Connection.ClientCertificate
                            };
                            await ValidateMachineCertificateAsync(validatedContext);
                            if (validatedContext.Result?.Succeeded == true)
                            {
                                context.Principal = validatedContext.Principal;
                                context.Success();
                            }
                        }
                    },
                    OnCertificateValidated = ValidateMachineCertificateAsync
                };
            });
        
        // 2. Authorization policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ActiveMachineOnly", policy =>
            {
                policy.AddAuthenticationSchemes(CertificateAuthenticationDefaults.AuthenticationScheme);
                policy.Requirements.Add(new ActiveMachineRequirement());
            });
        });

        return services;
    }


    private static Task ValidateMachineCertificateAsync(CertificateValidatedContext context)
    {
        var cert = context.ClientCertificate;


        // Debug logs to see what Kestrel receives
        Console.WriteLine($"[mTLS] Received client certificate: {cert?.Subject}");

        // 1. Get the URI from certificate Subject Alternative Name (SAN)
        var uriString = cert.GetNameInfo(X509NameType.UrlName, false);

        Console.WriteLine($"[mTLS] Extracted URI: '{uriString}'");

        // 2. Validate that it is a valid absolute URI
        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
        {
            context.Fail("Certificate does not contain a valid absolute URI.");
            return Task.CompletedTask;
        }

        // 3. Ensure the URI scheme is URN
        if (!string.Equals(uri.Scheme, "urn", StringComparison.OrdinalIgnoreCase))
        {
            context.Fail("Certificate URI scheme is not URN.");
            return Task.CompletedTask;
        }

        // 4. Ensure it belongs to the pinball machine namespace
        const string expectedPrefix = "pinball:machine:";
        if (!uri.AbsolutePath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            context.Fail("Certificate URN does not belong to pinball:machine namespace.");
            return Task.CompletedTask;
        }

        // 5. Extract and parse the machine GUID
        var rawGuid = uri.AbsolutePath.Substring(expectedPrefix.Length);
        if (!Guid.TryParse(rawGuid, out var machineId))
        {
            context.Fail("Certificate URN does not contain a valid machine GUID.");
            return Task.CompletedTask;
        }

        // 6. Attach the Machine claims identity
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, machineId.ToString()),
            new Claim(ClaimTypes.Role, "Machine")
        };

        context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
        context.Success();
        return Task.CompletedTask;
    }
}