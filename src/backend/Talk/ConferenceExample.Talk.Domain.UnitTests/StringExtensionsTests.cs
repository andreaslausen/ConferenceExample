using ConferenceExample.Talk.Domain.SharedKernel.Extensions;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Talk.Domain.UnitTests;

public class StringExtensionsTests
{
    [Fact]
    public void IsGuidV7_ValidGuidV7String_ReturnsTrue()
    {
        // Arrange
        var guidV7 = GuidV7.NewGuid();
        var guidString = guidV7.Value.ToString();

        // Act
        var result = guidString.IsGuidV7();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsGuidV7_ValidGuidV4String_ReturnsFalse()
    {
        // Arrange
        var guidV4 = Guid.NewGuid();
        var guidString = guidV4.ToString();

        // Act
        var result = guidString.IsGuidV7();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsGuidV7_InvalidGuidString_ReturnsFalse()
    {
        // Arrange
        var invalidGuid = "not-a-guid";

        // Act
        var result = invalidGuid.IsGuidV7();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsGuidV7_NullString_ReturnsFalse()
    {
        // Arrange
        string? nullString = null;

        // Act
        var result = nullString!.IsGuidV7();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsGuidV7_EmptyString_ReturnsFalse()
    {
        // Arrange
        var emptyString = string.Empty;

        // Act
        var result = emptyString.IsGuidV7();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsGuidV7_WhitespaceString_ReturnsFalse()
    {
        // Arrange
        var whitespace = "   ";

        // Act
        var result = whitespace.IsGuidV7();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsGuidV7_EmptyGuidString_ReturnsFalse()
    {
        // Arrange
        var emptyGuid = Guid.Empty.ToString();

        // Act
        var result = emptyGuid.IsGuidV7();

        // Assert
        Assert.False(result);
    }
}
