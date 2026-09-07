using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;

namespace ConferenceExample.Talk.Domain.UnitTests;

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
}
