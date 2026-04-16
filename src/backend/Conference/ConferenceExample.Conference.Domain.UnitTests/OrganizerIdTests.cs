using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Domain.UnitTests;

public class OrganizerIdTests
{
    [Fact]
    public void Constructor_ValidGuidV7_InitializesValue()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();

        // Act
        var organizerId = new OrganizerId(guidV7);

        // Assert
        Assert.Equal(guidV7, organizerId.Value);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();
        var id1 = new OrganizerId(guidV7);
        var id2 = new OrganizerId(guidV7);

        // Act & Assert
        Assert.Equal(id1, id2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var id1 = new OrganizerId(GuidV7.NewGuid());
        var id2 = new OrganizerId(GuidV7.NewGuid());

        // Act & Assert
        Assert.NotEqual(id1, id2);
    }
}
