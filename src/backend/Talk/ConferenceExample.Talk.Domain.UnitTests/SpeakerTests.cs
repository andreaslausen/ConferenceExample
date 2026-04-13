namespace ConferenceExample.Talk.Domain.UnitTests;

using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;

public class SpeakerTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsProperties()
    {
        // Arrange
        var id = new SpeakerId(GuidV7.NewGuid());
        var name = new Name("John", "Doe");
        var biography = new SpeakerBiography(
            "Experienced software architect with 10 years in the industry."
        );

        // Act
        var speaker = new Speaker(id, name, biography);

        // Assert
        Assert.Equal(id, speaker.Id);
        Assert.Equal(name, speaker.Name);
        Assert.Equal(biography, speaker.Biography);
    }

    [Fact]
    public void Constructor_ValidMinimalBiography_SetsProperties()
    {
        // Arrange
        var id = new SpeakerId(GuidV7.NewGuid());
        var name = new Name("Jane", "Smith");
        var biography = new SpeakerBiography("");

        // Act
        var speaker = new Speaker(id, name, biography);

        // Assert
        Assert.Equal("", speaker.Biography.Content);
    }
}
