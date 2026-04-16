using ConferenceExample.Conference.Domain.RoomManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Domain.UnitTests;

public class RoomTests
{
    [Fact]
    public void Constructor_ValidParameters_InitializesProperties()
    {
        // Arrange
        var id = new RoomId(GuidV7.NewGuid());
        var name = new Text("Conference Room A");

        // Act
        var room = new Room(id, name);

        // Assert
        Assert.Equal(id, room.Id);
        Assert.Equal(name, room.Name);
    }

    [Fact]
    public void Name_CanBeChanged()
    {
        // Arrange
        var id = new RoomId(GuidV7.NewGuid());
        var originalName = new Text("Conference Room A");
        var newName = new Text("Conference Room B");
        var room = new Room(id, originalName);

        // Act
        room.Name = newName;

        // Assert
        Assert.Equal(newName, room.Name);
    }

    [Fact]
    public void Id_IsReadOnly()
    {
        // Arrange
        var id = new RoomId(GuidV7.NewGuid());
        var name = new Text("Conference Room A");
        var room = new Room(id, name);

        // Assert
        Assert.Equal(id, room.Id);
        // Id cannot be changed (verified by compiler - no setter exists)
    }
}
