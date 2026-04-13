namespace ConferenceExample.Conference.Domain.UnitTests;

using ConferenceExample.Conference.Domain.SharedKernel.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public void IsGuidV7_ValidGuidV7String_ReturnsTrue()
    {
        // Arrange
        var guidV7 = Guid.CreateVersion7();
        var guidString = guidV7.ToString();

        // Act
        var result = guidString.IsGuidV7();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsGuidV7_NonGuidV7String_ReturnsFalse()
    {
        // Arrange
        var nonV7Guid = Guid.NewGuid(); // Creates a Version 4 GUID
        var guidString = nonV7Guid.ToString();

        // Act
        var result = guidString.IsGuidV7();

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
        var emptyString = "";

        // Act
        var result = emptyString.IsGuidV7();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsGuidV7_WhitespaceString_ReturnsFalse()
    {
        // Arrange
        var whitespaceString = "   ";

        // Act
        var result = whitespaceString.IsGuidV7();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsGuidV7_InvalidGuidString_ReturnsFalse()
    {
        // Arrange
        var invalidGuidString = "not-a-guid";

        // Act
        var result = invalidGuidString.IsGuidV7();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsGuidV7_PartialGuidString_ReturnsFalse()
    {
        // Arrange
        var partialGuidString = "12345678-1234-7234";

        // Act
        var result = partialGuidString.IsGuidV7();

        // Assert
        Assert.False(result);
    }
}
