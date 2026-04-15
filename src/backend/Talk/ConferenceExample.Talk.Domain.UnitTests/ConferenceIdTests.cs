using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Domain.UnitTests;

public class ConferenceIdTests
{
    [Fact]
    public void Constructor_ValidGuidV7_CreatesInstance()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();

        // Act
        var conferenceId = new ConferenceId(guidV7);

        // Assert
        Assert.Equal(guidV7, conferenceId.Value);
    }

    [Fact]
    public void Equality_SameValue_ReturnsTrue()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();
        var conferenceId1 = new ConferenceId(guidV7);
        var conferenceId2 = new ConferenceId(guidV7);

        // Act & Assert
        Assert.Equal(conferenceId1, conferenceId2);
        Assert.True(conferenceId1 == conferenceId2);
        Assert.False(conferenceId1 != conferenceId2);
    }

    [Fact]
    public void Equality_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var conferenceId1 = new ConferenceId(GuidV7.NewGuid());
        var conferenceId2 = new ConferenceId(GuidV7.NewGuid());

        // Act & Assert
        Assert.NotEqual(conferenceId1, conferenceId2);
        Assert.False(conferenceId1 == conferenceId2);
        Assert.True(conferenceId1 != conferenceId2);
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHashCode()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();
        var conferenceId1 = new ConferenceId(guidV7);
        var conferenceId2 = new ConferenceId(guidV7);

        // Act & Assert
        Assert.Equal(conferenceId1.GetHashCode(), conferenceId2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsValueString()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();
        var conferenceId = new ConferenceId(guidV7);

        // Act
        var result = conferenceId.ToString();

        // Assert
        Assert.Contains(guidV7.Value.ToString(), result);
    }
}
