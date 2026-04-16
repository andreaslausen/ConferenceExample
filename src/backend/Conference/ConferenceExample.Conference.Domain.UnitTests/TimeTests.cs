using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;

namespace ConferenceExample.Conference.Domain.UnitTests;

public class TimeTests
{
    [Fact]
    public void Constructor_ValidParameters_InitializesProperties()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(2);

        // Act
        var time = new Time(start, end);

        // Assert
        Assert.Equal(start, time.Start);
        Assert.Equal(end, time.End);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(2);
        var time1 = new Time(start, end);
        var time2 = new Time(start, end);

        // Act & Assert
        Assert.Equal(time1, time2);
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow;
        var time1 = new Time(start, start.AddHours(2));
        var time2 = new Time(start, start.AddHours(3));

        // Act & Assert
        Assert.NotEqual(time1, time2);
    }
}
