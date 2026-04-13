namespace ConferenceExample.Talk.Domain.UnitTests;

using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;

public class SpeakerIdTests
{
    [Fact]
    public void Constructor_ValidGuidV7_SetsProperty()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();

        // Act
        var speakerId = new SpeakerId(guidV7);

        // Assert
        Assert.Equal(guidV7, speakerId.Value);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();
        var speakerId1 = new SpeakerId(guidV7);
        var speakerId2 = new SpeakerId(guidV7);

        // Act & Assert
        Assert.Equal(speakerId1, speakerId2);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        // Arrange
        var speakerId1 = new SpeakerId(GuidV7.NewGuid());
        var speakerId2 = new SpeakerId(GuidV7.NewGuid());

        // Act & Assert
        Assert.NotEqual(speakerId1, speakerId2);
    }
}
