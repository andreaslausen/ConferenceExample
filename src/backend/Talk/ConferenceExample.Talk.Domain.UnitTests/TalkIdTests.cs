using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Domain.UnitTests;

public class TalkIdTests
{
    [Fact]
    public void Constructor_ValidGuidV7_CreatesInstance()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();

        // Act
        var talkId = new TalkId(guidV7);

        // Assert
        Assert.Equal(guidV7, talkId.Value);
    }

    [Fact]
    public void Equality_SameValue_ReturnsTrue()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();
        var talkId1 = new TalkId(guidV7);
        var talkId2 = new TalkId(guidV7);

        // Act & Assert
        Assert.Equal(talkId1, talkId2);
        Assert.True(talkId1 == talkId2);
        Assert.False(talkId1 != talkId2);
    }

    [Fact]
    public void Equality_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var talkId1 = new TalkId(GuidV7.NewGuid());
        var talkId2 = new TalkId(GuidV7.NewGuid());

        // Act & Assert
        Assert.NotEqual(talkId1, talkId2);
        Assert.False(talkId1 == talkId2);
        Assert.True(talkId1 != talkId2);
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHashCode()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();
        var talkId1 = new TalkId(guidV7);
        var talkId2 = new TalkId(guidV7);

        // Act & Assert
        Assert.Equal(talkId1.GetHashCode(), talkId2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsValueString()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();
        var talkId = new TalkId(guidV7);

        // Act
        var result = talkId.ToString();

        // Assert
        Assert.Contains(guidV7.Value.ToString(), result);
    }
}
