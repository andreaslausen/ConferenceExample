namespace ConferenceExample.Talk.Domain.UnitTests;

using ConferenceExample.Talk.Domain.SpeakerManagement;

public class SpeakerBiographyTests
{
    [Fact]
    public void Constructor_ValidBiography_SetsProperty()
    {
        // Act
        var biography = new SpeakerBiography(
            "John is a passionate developer with expertise in .NET and cloud technologies."
        );

        // Assert
        Assert.Equal(
            "John is a passionate developer with expertise in .NET and cloud technologies.",
            biography.Content
        );
    }

    [Fact]
    public void Constructor_EmptyString_DoesNotThrow()
    {
        // Act
        var biography = new SpeakerBiography("");

        // Assert
        Assert.Equal("", biography.Content);
    }

    [Fact]
    public void Constructor_BiographyExceeds2000Characters_ThrowsArgumentException()
    {
        // Arrange
        var tooLong = new string('x', 2001);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new SpeakerBiography(tooLong));
        Assert.Contains("The biography must have a maximum of 2000 characters", exception.Message);
    }

    [Fact]
    public void Constructor_BiographyExactly2000Characters_DoesNotThrow()
    {
        // Arrange
        var exactly2000 = new string('x', 2000);

        // Act
        var biography = new SpeakerBiography(exactly2000);

        // Assert
        Assert.Equal(exactly2000, biography.Content);
    }

    [Fact]
    public void Constructor_BiographyWith1999Characters_DoesNotThrow()
    {
        // Arrange
        var exactly1999 = new string('x', 1999);

        // Act
        var biography = new SpeakerBiography(exactly1999);

        // Assert
        Assert.Equal(exactly1999, biography.Content);
    }
}
