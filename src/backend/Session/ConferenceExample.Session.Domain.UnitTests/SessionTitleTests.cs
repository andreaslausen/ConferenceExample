namespace ConferenceExample.Session.Domain.UnitTests;

using ConferenceExample.Session.Domain.ValueObjects;

public class SessionTitleTests
{
    [Fact]
    public void Constructor_ValidTitle_SetsProperty()
    {
        // Act
        var title = new SessionTitle("Clean Architecture in Practice");

        // Assert
        Assert.Equal("Clean Architecture in Practice", title.Title);
    }

    [Fact]
    public void Constructor_TitleExceeds100Characters_ThrowsArgumentException()
    {
        // Arrange
        var tooLong = new string('x', 101);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SessionTitle(tooLong));
    }

    [Fact]
    public void Constructor_TitleExactly100Characters_DoesNotThrow()
    {
        // Arrange
        var exactly100 = new string('x', 100);

        // Act & Assert
        var title = new SessionTitle(exactly100);
        Assert.Equal(exactly100, title.Title);
    }
}
