using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using ConferenceExample.Authentication;
using ConferenceExample.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace ConferenceExample.IntegrationTests;

[Collection("IntegrationTests")]
public class AuthControllerTests : IntegrationTestBase
{
    public AuthControllerTests(IntegrationTestWebApplicationFactory factory)
        : base(factory) { }

    [Theory]
    [InlineData(UserRole.Speaker)]
    [InlineData(UserRole.Organizer)]
    [InlineData(UserRole.Attendee)]
    public async Task Register_WithValidData_ReturnsTokenAndSuccess(UserRole role)
    {
        // Arrange
        var email = GetUniqueEmail("register");
        var password = "SecurePassword123!";
        var registerDto = new
        {
            email,
            password,
            role = (int)role,
        };

        // Act
        var response = await PostAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponse<RegisterResponseDto>(response);
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();

        // Verify token contains correct claims
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        token
            .Claims.Should()
            .Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == email);
        token.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == role.ToString());
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        var email = GetUniqueEmail("duplicate");
        var password = "SecurePassword123!";
        var registerDto = new
        {
            email,
            password,
            role = (int)UserRole.Speaker,
        };

        // First registration
        await PostAsync("/api/auth/register", registerDto);

        // Act - Try to register again with same email
        var response = await PostAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("User already exists");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var email = GetUniqueEmail("login");
        var password = "SecurePassword123!";
        var registerDto = new
        {
            email,
            password,
            role = (int)UserRole.Speaker,
        };
        await PostAsync("/api/auth/register", registerDto);

        var loginDto = new { email, password };

        // Act
        var response = await PostAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponse<LoginResponseDto>(response);
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();

        // Verify token contains correct claims
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        token
            .Claims.Should()
            .Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == email);
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var loginDto = new { email = "nonexistent@test.com", password = "SomePassword123!" };

        // Act
        var response = await PostAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsBadRequest()
    {
        // Arrange
        var email = GetUniqueEmail("wrongpass");
        var password = "CorrectPassword123!";
        var registerDto = new
        {
            email,
            password,
            role = (int)UserRole.Speaker,
        };
        await PostAsync("/api/auth/register", registerDto);

        var loginDto = new { email, password = "WrongPassword123!" };

        // Act
        var response = await PostAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task RegisteredToken_CanBeUsedForAuthenticatedEndpoints()
    {
        // Arrange
        var email = GetUniqueEmail("tokentest");
        var password = "SecurePassword123!";
        var token = await GetAuthenticationToken(email, password, UserRole.Speaker);

        // Act
        SetAuthenticationToken(token);
        var response = await HttpClient.GetAsync("/api/talks/my-talks");

        // Assert - Should succeed (200 OK with empty list) rather than 401 Unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AuthenticatedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        // Act - Try to access protected endpoint without token
        var response = await HttpClient.GetAsync("/api/talks/my-talks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private record RegisterResponseDto(string Token);

    private record LoginResponseDto(string Token);
}
