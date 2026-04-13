using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ConferenceExample.Authentication.SharedKernel.ValueObjects.Ids;
using Microsoft.IdentityModel.Tokens;

namespace ConferenceExample.Authentication;

public class AuthenticationService(IUserRepository userRepository, JwtSettings jwtSettings)
    : IAuthenticationService
{
    public async Task<AuthenticationResult> Register(string email, string password, UserRole role)
    {
        // Check if user already exists
        var existingUser = await userRepository.GetByEmail(email);
        if (existingUser is not null)
        {
            return new AuthenticationResult(false, null, "User already exists");
        }

        // Create new user
        var user = new User
        {
            Id = new UserId(GuidV7.NewGuid()),
            Email = email,
            PasswordHash = HashPassword(password),
            Role = role,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        await userRepository.Add(user);

        // Generate JWT token
        var token = GenerateJwtToken(user);

        return new AuthenticationResult(true, token, null);
    }

    public async Task<AuthenticationResult> Login(string email, string password)
    {
        var user = await userRepository.GetByEmail(email);
        if (user is null)
        {
            return new AuthenticationResult(false, null, "Invalid credentials");
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            return new AuthenticationResult(false, null, "Invalid credentials");
        }

        var token = GenerateJwtToken(user);

        return new AuthenticationResult(true, token, null);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.Value.Value.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, GuidV7.NewGuid().Value.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(jwtSettings.ExpirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        // Simple SHA256 hash - not production-ready, but sufficient for demo
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        var computedHash = HashPassword(password);
        return computedHash == hash;
    }
}
