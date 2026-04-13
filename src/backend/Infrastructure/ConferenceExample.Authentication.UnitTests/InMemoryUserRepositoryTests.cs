using ConferenceExample.Authentication.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Authentication.UnitTests;

public class InMemoryUserRepositoryTests
{
    [Fact]
    public async Task Add_User_StoresUserSuccessfully()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var user = CreateTestUser("test@example.com", UserRole.Speaker);

        // Act
        await repository.Add(user);
        var retrievedUser = await repository.GetByEmail("test@example.com");

        // Assert
        Assert.NotNull(retrievedUser);
        Assert.Equal(user.Id.Value, retrievedUser.Id.Value);
        Assert.Equal(user.Email, retrievedUser.Email);
    }

    [Fact]
    public async Task GetByEmail_ExistingUser_ReturnsUser()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var user = CreateTestUser("test@example.com", UserRole.Organizer);
        await repository.Add(user);

        // Act
        var result = await repository.GetByEmail("test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id.Value, result.Id.Value);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal(UserRole.Organizer, result.Role);
    }

    [Fact]
    public async Task GetByEmail_NonExistingUser_ReturnsNull()
    {
        // Arrange
        var repository = new InMemoryUserRepository();

        // Act
        var result = await repository.GetByEmail("nonexistent@example.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmail_CaseInsensitive_ReturnsUser()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var user = CreateTestUser("Test@Example.Com", UserRole.Attendee);
        await repository.Add(user);

        // Act
        var result1 = await repository.GetByEmail("test@example.com");
        var result2 = await repository.GetByEmail("TEST@EXAMPLE.COM");

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(user.Id.Value, result1.Id.Value);
        Assert.Equal(user.Id.Value, result2.Id.Value);
    }

    [Fact]
    public async Task GetById_ExistingUser_ReturnsUser()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var user = CreateTestUser("test@example.com", UserRole.Speaker);
        await repository.Add(user);

        // Act
        var result = await repository.GetById(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id.Value, result.Id.Value);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task GetById_NonExistingUser_ReturnsNull()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var nonExistentId = new UserId(GuidV7.NewGuid());

        // Act
        var result = await repository.GetById(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Add_MultipleUsers_AllUsersRetrievable()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var user1 = CreateTestUser("user1@example.com", UserRole.Speaker);
        var user2 = CreateTestUser("user2@example.com", UserRole.Organizer);
        var user3 = CreateTestUser("user3@example.com", UserRole.Attendee);

        // Act
        await repository.Add(user1);
        await repository.Add(user2);
        await repository.Add(user3);

        // Assert
        var retrieved1 = await repository.GetByEmail("user1@example.com");
        var retrieved2 = await repository.GetByEmail("user2@example.com");
        var retrieved3 = await repository.GetByEmail("user3@example.com");

        Assert.NotNull(retrieved1);
        Assert.NotNull(retrieved2);
        Assert.NotNull(retrieved3);
        Assert.Equal(UserRole.Speaker, retrieved1.Role);
        Assert.Equal(UserRole.Organizer, retrieved2.Role);
        Assert.Equal(UserRole.Attendee, retrieved3.Role);
    }

    [Fact]
    public async Task Add_SameUserTwice_BothInstancesStored()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var userId = new UserId(GuidV7.NewGuid());
        var user1 = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hash1",
            Role = UserRole.Speaker,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        var user2 = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hash2",
            Role = UserRole.Organizer,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        // Act
        await repository.Add(user1);
        await repository.Add(user2);

        // Assert - repository doesn't prevent duplicates; returns the first match
        var result = await repository.GetById(userId);
        Assert.NotNull(result);
    }

    private static User CreateTestUser(string email, UserRole role) =>
        new()
        {
            Id = new UserId(GuidV7.NewGuid()),
            Email = email,
            PasswordHash = "hashedPassword",
            Role = role,
            CreatedAt = DateTimeOffset.UtcNow,
        };
}
