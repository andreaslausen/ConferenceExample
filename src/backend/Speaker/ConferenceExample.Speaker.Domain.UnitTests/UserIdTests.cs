using ConferenceExample.Speaker.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Speaker.Domain.SpeakerManagement;

namespace ConferenceExample.Speaker.Domain.UnitTests;

public class UserIdTests
{
    [Fact]
    public void Constructor_ValidGuidV7_SetsProperty()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();

        // Act
        var userId = new UserId(guidV7);

        // Assert
        Assert.Equal(guidV7, userId.Value);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();

        // Act & Assert
        Assert.Equal(new UserId(guidV7), new UserId(guidV7));
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        // Act & Assert
        Assert.NotEqual(new UserId(GuidV7.NewGuid()), new UserId(GuidV7.NewGuid()));
    }

    [Fact]
    public void UserId_AndSpeakerId_AreDistinctTypes()
    {
        // A speaker profile has its own identity so one account can hold several role profiles;
        // the two ids must not be interchangeable even when they wrap the same value.
        var guidV7 = GuidV7.NewGuid();

        var userId = new UserId(guidV7);
        var speakerId = new SpeakerId(guidV7);

        Assert.NotEqual<object>(userId, speakerId);
    }
}
