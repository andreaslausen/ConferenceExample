namespace ConferenceExample.Talk.Domain.UnitTests;

using ConferenceExample.Talk.Domain.TalkManagement;

public class TalkTagTests
{
    [Fact]
    public void Constructor_ValidTag_SetsProperty()
    {
        // Act
        var tag = new TalkTag("dotnet");

        // Assert
        Assert.Equal("dotnet", tag.Tag);
    }

    [Fact]
    public void Constructor_TagExceeds20Characters_ThrowsArgumentException()
    {
        // Arrange
        var tooLong = new string('x', 21);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TalkTag(tooLong));
    }

    [Fact]
    public void Constructor_TagExactly20Characters_DoesNotThrow()
    {
        // Arrange
        var exactly20 = new string('x', 20);

        // Act & Assert
        var tag = new TalkTag(exactly20);
        Assert.Equal(exactly20, tag.Tag);
    }
}
