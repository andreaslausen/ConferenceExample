namespace ConferenceExample.Talk.Domain.UnitTests;

using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects;

public class TextTests
{
    [Fact]
    public void Constructor_ValidText_SetsProperty()
    {
        // Act
        var text = new Text("Domain-Driven Design");

        // Assert
        Assert.Equal("Domain-Driven Design", text.Value);
    }

    [Fact]
    public void Constructor_NullValue_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Text(null!));
        Assert.Contains("Value cannot be null or whitespace.", exception.Message);
    }

    [Fact]
    public void Constructor_EmptyString_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Text(""));
        Assert.Contains("Value cannot be null or whitespace.", exception.Message);
    }

    [Fact]
    public void Constructor_WhitespaceOnly_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Text("   "));
        Assert.Contains("Value cannot be null or whitespace.", exception.Message);
    }

    [Fact]
    public void Constructor_TabsOnly_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Text("\t\t\t"));
        Assert.Contains("Value cannot be null or whitespace.", exception.Message);
    }

    [Fact]
    public void Constructor_NewlinesOnly_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Text("\n\n"));
        Assert.Contains("Value cannot be null or whitespace.", exception.Message);
    }
}
