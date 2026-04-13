namespace ConferenceExample.Talk.Domain.UnitTests;

using ConferenceExample.Talk.Domain.TalkManagement;

public class TalkTitleTests
{
    [Fact]
    public void Constructor_ValidTitle_SetsProperty()
    {
        // Act
        var title = new TalkTitle("Clean Architecture in Practice");

        // Assert
        Assert.Equal("Clean Architecture in Practice", title.Title);
    }

    [Fact]
    public void Constructor_TitleExceeds100Characters_ThrowsArgumentException()
    {
        // Arrange
        var tooLong = new string('x', 101);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TalkTitle(tooLong));
    }

    [Fact]
    public void Constructor_TitleExactly100Characters_DoesNotThrow()
    {
        // Arrange
        var exactly100 = new string('x', 100);

        // Act & Assert
        var title = new TalkTitle(exactly100);
        Assert.Equal(exactly100, title.Title);
    }
}
