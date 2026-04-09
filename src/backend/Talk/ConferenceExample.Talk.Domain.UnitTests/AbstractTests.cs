namespace ConferenceExample.Talk.Domain.UnitTests;

using ConferenceExample.Talk.Domain.ValueObjects;

public class AbstractTests
{
    [Fact]
    public void Constructor_ValidAbstract_SetsProperty()
    {
        // Act
        var @abstract = new Abstract("This session covers event sourcing patterns.");

        // Assert
        Assert.Equal("This session covers event sourcing patterns.", @abstract.Content);
    }

    [Fact]
    public void Constructor_AbstractExceeds1000Characters_ThrowsArgumentException()
    {
        // Arrange
        var tooLong = new string('x', 1001);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Abstract(tooLong));
    }

    [Fact]
    public void Constructor_AbstractExactly1000Characters_DoesNotThrow()
    {
        // Arrange
        var exactly1000 = new string('x', 1000);

        // Act & Assert
        var @abstract = new Abstract(exactly1000);
        Assert.Equal(exactly1000, @abstract.Content);
    }
}
