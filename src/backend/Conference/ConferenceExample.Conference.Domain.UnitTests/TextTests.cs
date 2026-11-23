namespace ConferenceExample.Conference.Domain.UnitTests;

using ConferenceExample.Conference.Domain.ValueObjects;

public class TextTests
{
    [Fact]
    public void Constructor_ValidText_SetsProperty()
    {
        // Act
        var text = new Text("DDD Europe 2026");

        // Assert
        Assert.Equal("DDD Europe 2026", text.Value);
    }

    [Fact]
    public void Constructor_NullValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Text(null!));
    }

    [Fact]
    public void Constructor_EmptyString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Text(""));
    }

    [Fact]
    public void Constructor_WhitespaceOnly_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Text("   "));
    }
}
