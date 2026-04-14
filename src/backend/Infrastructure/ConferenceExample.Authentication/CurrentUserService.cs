using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ConferenceExample.Authentication.SharedKernel.Extensions;
using ConferenceExample.Authentication.SharedKernel.ValueObjects.Ids;
using Microsoft.AspNetCore.Http;

namespace ConferenceExample.Authentication;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private const string UserIdClaimType = JwtRegisteredClaimNames.Sub;
    private const string EmailClaimType = JwtRegisteredClaimNames.Email;
    private const string RoleClaimType = ClaimTypes.Role;

    public UserId GetCurrentUserId()
    {
        var userIdClaim = GetClaim(UserIdClaimType);

        if (!userIdClaim.IsGuidV7())
        {
            throw new UnauthorizedAccessException("Invalid user ID in token.");
        }

        return new UserId(GuidV7.Parse(userIdClaim));
    }

    public UserRole GetCurrentUserRole()
    {
        var roleClaim = GetClaim(RoleClaimType);

        if (!Enum.TryParse<UserRole>(roleClaim, out var role))
        {
            throw new UnauthorizedAccessException("Invalid role in token.");
        }

        return role;
    }

    public string GetCurrentUserEmail()
    {
        return GetClaim(EmailClaimType);
    }

    public bool IsAuthenticated()
    {
        return httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

    private string GetClaim(string claimType)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var claim = httpContext.User.FindFirst(claimType);

        if (claim is null || string.IsNullOrWhiteSpace(claim.Value))
        {
            throw new UnauthorizedAccessException($"Claim '{claimType}' not found in user token.");
        }

        return claim.Value;
    }
}
