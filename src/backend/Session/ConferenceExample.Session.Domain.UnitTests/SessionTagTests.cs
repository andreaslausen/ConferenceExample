namespace ConferenceExample.Session.Domain.UnitTests;

using ConferenceExample.Session.Domain.ValueObjects;

public class SessionTagTests
{
    [Fact]
    public void Constructor_ValidTag_SetsProperty()
    {
        // Act
        var tag = new SessionTag("dotnet");

        // Assert
        Assert.Equal("dotnet", tag.Tag);
    }

    [Fact]
    public void Constructor_TagExceeds20Characters_ThrowsArgumentException()
    {
        // Arrange
        var tooLong = new string('x', 21);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SessionTag(tooLong));
    }

    [Fact]
    public void Constructor_TagExactly20Characters_DoesNotThrow()
    {
        // Arrange
        var exactly20 = new string('x', 20);

        // Act & Assert
        var tag = new SessionTag(exactly20);
        Assert.Equal(exactly20, tag.Tag);
    }
}
