using ConferenceExample.Talk.Domain.SharedKernel.Extensions;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Talk.Domain.UnitTests;

public class GuidExtensionsTests
{
    [Fact]
    public void IsGuidV7_ValidGuidV7_ReturnsTrue()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();

        // Act
        var result = guidV7.Value.IsGuidV7();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsGuidV7_GuidV4_ReturnsFalse()
    {
        // Arrange
        var guidV4 = Guid.NewGuid();

        // Act
        var result = guidV4.IsGuidV7();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsGuidV7_EmptyGuid_ReturnsFalse()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var result = emptyGuid.IsGuidV7();

        // Assert
        Assert.False(result);
    }
}
