namespace ConferenceExample.Conference.Domain.UnitTests;

using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

public class GuidV7Tests
{
    [Fact]
    public void Constructor_ValidGuidV7_SetsProperty()
    {
        // Arrange
        var guidV7Value = Guid.CreateVersion7();

        // Act
        var guidV7 = new GuidV7(guidV7Value);

        // Assert
        Assert.Equal(guidV7Value, guidV7.Value);
    }

    [Fact]
    public void Constructor_NonGuidV7_ThrowsArgumentException()
    {
        // Arrange
        var nonV7Guid = Guid.NewGuid(); // Creates a Version 4 GUID

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new GuidV7(nonV7Guid));
        Assert.Equal("value", exception.ParamName);
        Assert.Contains("Value must be a valid UUIDv7.", exception.Message);
    }

    [Fact]
    public void NewGuid_CreatesValidGuidV7()
    {
        // Act
        var guidV7 = GuidV7.NewGuid();

        // Assert
        Assert.NotNull(guidV7);
        Assert.Equal(7, guidV7.Value.Version);
    }

    [Fact]
    public void Parse_ValidGuidV7String_ReturnsGuidV7()
    {
        // Arrange
        var guidV7Value = Guid.CreateVersion7();
        var guidString = guidV7Value.ToString();

        // Act
        var guidV7 = GuidV7.Parse(guidString);

        // Assert
        Assert.Equal(guidV7Value, guidV7.Value);
    }

    [Fact]
    public void Parse_NonGuidV7String_ThrowsArgumentException()
    {
        // Arrange
        var nonV7Guid = Guid.NewGuid(); // Creates a Version 4 GUID
        var guidString = nonV7Guid.ToString();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => GuidV7.Parse(guidString));
        Assert.Equal("value", exception.ParamName);
        Assert.Contains("Value must be a valid UUIDv7 string.", exception.Message);
    }

    [Fact]
    public void Parse_InvalidGuidString_ThrowsException()
    {
        // Arrange
        var invalidGuidString = "not-a-guid";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => GuidV7.Parse(invalidGuidString));
    }

    [Fact]
    public void Parse_NullString_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => GuidV7.Parse(null!));
    }

    [Fact]
    public void Parse_EmptyString_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => GuidV7.Parse(""));
    }

    [Fact]
    public void Parse_WhitespaceString_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => GuidV7.Parse("   "));
    }

    [Fact]
    public void ImplicitOperatorToGuid_ConvertsCorrectly()
    {
        // Arrange
        var guidV7Value = Guid.CreateVersion7();
        var guidV7 = new GuidV7(guidV7Value);

        // Act
        Guid result = guidV7;

        // Assert
        Assert.Equal(guidV7Value, result);
    }

    [Fact]
    public void ImplicitOperatorFromGuid_ConvertsCorrectly()
    {
        // Arrange
        var guidV7Value = Guid.CreateVersion7();

        // Act
        GuidV7 guidV7 = guidV7Value;

        // Assert
        Assert.Equal(guidV7Value, guidV7.Value);
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        // Arrange
        var guidV7Value = Guid.CreateVersion7();
        var guidV7 = new GuidV7(guidV7Value);

        // Act
        var result = guidV7.ToString();

        // Assert
        Assert.Equal(guidV7Value.ToString(), result);
    }
}
