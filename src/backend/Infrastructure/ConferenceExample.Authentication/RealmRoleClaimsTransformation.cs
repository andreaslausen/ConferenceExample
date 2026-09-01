using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace ConferenceExample.Authentication;

/// <summary>
/// Keycloak puts realm roles in a nested "realm_access.roles" claim rather than individual
/// role claims. This flattens the entries that match a known <see cref="UserRole"/> into
/// ClaimTypes.Role claims, skipping Keycloak's built-in default roles (offline_access,
/// uma_authorization, default-roles-*) that every user carries.
/// </summary>
public class RealmRoleClaimsTransformation : IClaimsTransformation
{
    private const string RealmAccessClaimType = "realm_access";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity { IsAuthenticated: true } identity)
        {
            return Task.FromResult(principal);
        }

        var realmAccessClaim = identity.FindFirst(RealmAccessClaimType);
        if (realmAccessClaim is null)
        {
            return Task.FromResult(principal);
        }

        using var realmAccess = JsonDocument.Parse(realmAccessClaim.Value);
        if (!realmAccess.RootElement.TryGetProperty("roles", out var roles))
        {
            return Task.FromResult(principal);
        }

        foreach (var role in roles.EnumerateArray())
        {
            var roleName = role.GetString();
            if (roleName is null || !Enum.TryParse<UserRole>(roleName, out _))
            {
                continue;
            }

            if (identity.FindFirst(c => c.Type == ClaimTypes.Role && c.Value == roleName) is null)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
            }
        }

        return Task.FromResult(principal);
    }
}
