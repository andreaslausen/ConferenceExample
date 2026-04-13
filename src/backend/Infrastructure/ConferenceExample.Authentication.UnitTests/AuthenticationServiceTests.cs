using ConferenceExample.Authentication.SharedKernel.ValueObjects.Ids;
using NSubstitute;

namespace ConferenceExample.Authentication.UnitTests;

public class AuthenticationServiceTests
{
    [Fact]
    public async Task Register_NewUser_ReturnsSuccessWithToken()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var jwtSettings = CreateJwtSettings();
        var service = new AuthenticationService(userRepository, jwtSettings);

        userRepository.GetByEmail("test@example.com").Returns(Task.FromResult<User?>(null));

        // Act
        var result = await service.Register("test@example.com", "password123", UserRole.Speaker);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        Assert.Null(result.ErrorMessage);
        await userRepository.Received(1).Add(Arg.Any<User>());
    }

    [Fact]
    public async Task Register_ExistingUser_ReturnsErrorMessage()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var jwtSettings = CreateJwtSettings();
        var service = new AuthenticationService(userRepository, jwtSettings);

        var existingUser = new User
        {
            Id = new UserId(GuidV7.NewGuid()),
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = UserRole.Speaker,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        userRepository.GetByEmail("test@example.com").Returns(Task.FromResult<User?>(existingUser));

        // Act
        var result = await service.Register("test@example.com", "password123", UserRole.Speaker);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Token);
        Assert.Equal("User already exists", result.ErrorMessage);
        await userRepository.DidNotReceive().Add(Arg.Any<User>());
    }

    [Fact]
    public async Task Register_CreatesUserWithCorrectProperties()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var jwtSettings = CreateJwtSettings();
        var service = new AuthenticationService(userRepository, jwtSettings);
        User? capturedUser = null;

        userRepository.GetByEmail("test@example.com").Returns(Task.FromResult<User?>(null));
        userRepository.Add(Arg.Do<User>(u => capturedUser = u)).Returns(Task.CompletedTask);

        // Act
        await service.Register("test@example.com", "password123", UserRole.Organizer);

        // Assert
        Assert.NotNull(capturedUser);
        Assert.Equal("test@example.com", capturedUser.Email);
        Assert.Equal(UserRole.Organizer, capturedUser.Role);
        Assert.NotEmpty(capturedUser.PasswordHash);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var jwtSettings = CreateJwtSettings();
        var tempService = new AuthenticationService(userRepository, jwtSettings);

        // Create a user with a known password hash
        var user = new User
        {
            Id = new UserId(GuidV7.NewGuid()),
            Email = "test@example.com",
            PasswordHash = HashPassword("password123"),
            Role = UserRole.Speaker,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        userRepository.GetByEmail("test@example.com").Returns(Task.FromResult<User?>(user));

        var service = new AuthenticationService(userRepository, jwtSettings);

        // Act
        var result = await service.Login("test@example.com", "password123");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task Login_UserNotFound_ReturnsErrorMessage()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var jwtSettings = CreateJwtSettings();
        var service = new AuthenticationService(userRepository, jwtSettings);

        userRepository.GetByEmail("nonexistent@example.com").Returns(Task.FromResult<User?>(null));

        // Act
        var result = await service.Login("nonexistent@example.com", "password123");

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Token);
        Assert.Equal("Invalid credentials", result.ErrorMessage);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsErrorMessage()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var jwtSettings = CreateJwtSettings();
        var service = new AuthenticationService(userRepository, jwtSettings);

        var user = new User
        {
            Id = new UserId(GuidV7.NewGuid()),
            Email = "test@example.com",
            PasswordHash = HashPassword("correctpassword"),
            Role = UserRole.Speaker,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        userRepository.GetByEmail("test@example.com").Returns(Task.FromResult<User?>(user));

        // Act
        var result = await service.Login("test@example.com", "wrongpassword");

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Token);
        Assert.Equal("Invalid credentials", result.ErrorMessage);
    }

    private static JwtSettings CreateJwtSettings() =>
        new()
        {
            Secret = "ThisIsASecretKeyForTestingPurposesOnly12345",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationHours = 24,
        };

    // Helper method to hash passwords the same way as AuthenticationService
    private static string HashPassword(string password)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(password)
        );
        return Convert.ToBase64String(bytes);
    }
}
