using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConferenceExample.Authentication;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        var keycloakSettings =
            configuration.GetSection("Keycloak").Get<KeycloakSettings>()
            ?? throw new InvalidOperationException("Keycloak settings not found in configuration");

        services.AddSingleton(keycloakSettings);
        services.AddHttpContextAccessor();
        services.AddScoped<CurrentUserService>();
        services.AddScoped<Conference.Application.ICurrentUserService>(sp =>
            sp.GetRequiredService<CurrentUserService>()
        );
        services.AddScoped<Talk.Application.ICurrentUserService>(sp =>
            sp.GetRequiredService<CurrentUserService>()
        );
        services.AddTransient<IClaimsTransformation, RealmRoleClaimsTransformation>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = keycloakSettings.Authority;
                options.Audience = keycloakSettings.Audience;
                // Keycloak runs over plain HTTP in local development.
                options.RequireHttpsMetadata = !environment.IsDevelopment();

                options.TokenValidationParameters.ValidIssuer = keycloakSettings.Authority;
                options.TokenValidationParameters.NameClaimType = "sub";
                options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;

                // Disable default claim mapping to preserve JWT standard claim names (e.g. "sub").
                options.MapInboundClaims = false;
            });

        services.AddAuthorization();

        return services;
    }
}
