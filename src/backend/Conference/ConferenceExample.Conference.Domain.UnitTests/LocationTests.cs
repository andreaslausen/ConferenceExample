using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;

namespace ConferenceExample.Conference.Domain.UnitTests;

public class LocationTests
{
    [Fact]
    public void Constructor_ValidParameters_InitializesProperties()
    {
        // Arrange
        var name = new Text("Convention Center");
        var address = new Address("123 Main St", "Springfield", "IL", "62701", "USA");

        // Act
        var location = new Location(name, address);

        // Assert
        Assert.Equal(name, location.Name);
        Assert.Equal(address, location.Address);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var name = new Text("Convention Center");
        var address = new Address("123 Main St", "Springfield", "IL", "62701", "USA");
        var location1 = new Location(name, address);
        var location2 = new Location(name, address);

        // Act & Assert
        Assert.Equal(location1, location2);
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var name1 = new Text("Convention Center");
        var name2 = new Text("Conference Hall");
        var address = new Address("123 Main St", "Springfield", "IL", "62701", "USA");
        var location1 = new Location(name1, address);
        var location2 = new Location(name2, address);

        // Act & Assert
        Assert.NotEqual(location1, location2);
    }
}
