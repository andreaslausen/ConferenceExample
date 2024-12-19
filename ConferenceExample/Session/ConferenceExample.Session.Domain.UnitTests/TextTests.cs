using ConferenceExample.Session.Domain.ValueObjects;

namespace ConferenceExample.Session.Domain.UnitTests;

public class TextTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenValueIsNull()
    {
        // Arrange
        string value = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Text(value));
        Assert.Equal("Value cannot be null or whitespace. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenValueIsWhitespace()
    {
        // Arrange
        string value = "   ";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Text(value));
        Assert.Equal("Value cannot be null or whitespace. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldSetValue_WhenValueIsValid()
    {
        // Arrange
        string value = "Valid text";

        // Act
        var text = new Text(value);

        // Assert
        Assert.Equal(value, text.Value);
    }
}