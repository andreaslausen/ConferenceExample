using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Domain.UnitTests;

public class TalkTypeIdTests
{
    [Fact]
    public void Constructor_ValidGuidV7_CreatesInstance()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();

        // Act
        var talkTypeId = new TalkTypeId(guidV7);

        // Assert
        Assert.Equal(guidV7, talkTypeId.Value);
    }

    [Fact]
    public void Equality_SameValue_ReturnsTrue()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();
        var talkTypeId1 = new TalkTypeId(guidV7);
        var talkTypeId2 = new TalkTypeId(guidV7);

        // Act & Assert
        Assert.Equal(talkTypeId1, talkTypeId2);
        Assert.True(talkTypeId1 == talkTypeId2);
        Assert.False(talkTypeId1 != talkTypeId2);
    }

    [Fact]
    public void Equality_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var talkTypeId1 = new TalkTypeId(GuidV7.NewGuid());
        var talkTypeId2 = new TalkTypeId(GuidV7.NewGuid());

        // Act & Assert
        Assert.NotEqual(talkTypeId1, talkTypeId2);
        Assert.False(talkTypeId1 == talkTypeId2);
        Assert.True(talkTypeId1 != talkTypeId2);
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHashCode()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();
        var talkTypeId1 = new TalkTypeId(guidV7);
        var talkTypeId2 = new TalkTypeId(guidV7);

        // Act & Assert
        Assert.Equal(talkTypeId1.GetHashCode(), talkTypeId2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsValueString()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();
        var talkTypeId = new TalkTypeId(guidV7);

        // Act
        var result = talkTypeId.ToString();

        // Assert
        Assert.Contains(guidV7.Value.ToString(), result);
    }
}
