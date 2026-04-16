using ConferenceExample.Conference.Domain.RoomManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Domain.UnitTests;

public class RoomIdTests
{
    [Fact]
    public void Constructor_ValidGuidV7_InitializesValue()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();

        // Act
        var roomId = new RoomId(guidV7);

        // Assert
        Assert.Equal(guidV7, roomId.Value);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();
        var id1 = new RoomId(guidV7);
        var id2 = new RoomId(guidV7);

        // Act & Assert
        Assert.Equal(id1, id2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var id1 = new RoomId(GuidV7.NewGuid());
        var id2 = new RoomId(GuidV7.NewGuid());

        // Act & Assert
        Assert.NotEqual(id1, id2);
    }
}
