namespace ConferenceExample.EventStore.UnitTests;

public class ConcurrencyExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        // Arrange
        const string expectedMessage = "Concurrency conflict detected";

        // Act
        var exception = new ConcurrencyException(expectedMessage);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsBoth()
    {
        // Arrange
        const string expectedMessage = "Concurrency conflict detected";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new ConcurrencyException(expectedMessage, innerException);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void ConcurrencyException_InheritsFromException()
    {
        // Arrange
        var exception = new ConcurrencyException("Test");

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void Constructor_WithNullMessage_DoesNotThrow()
    {
        // Act & Assert
        var exception = new ConcurrencyException(null!);

        Assert.NotNull(exception);
    }

    [Fact]
    public void Constructor_WithEmptyMessage_SetsEmptyMessage()
    {
        // Arrange
        const string expectedMessage = "";

        // Act
        var exception = new ConcurrencyException(expectedMessage);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndNullInnerException_SetsMessageOnly()
    {
        // Arrange
        const string expectedMessage = "Concurrency conflict";

        // Act
        var exception = new ConcurrencyException(expectedMessage, null!);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void Throw_ConcurrencyException_CanBeCaught()
    {
        // Arrange
        void ThrowException() => throw new ConcurrencyException("Test exception");

        // Act & Assert
        var exception = Assert.Throws<ConcurrencyException>(ThrowException);

        Assert.NotNull(exception);
        Assert.Equal("Test exception", exception.Message);
    }

    [Fact]
    public void ConcurrencyException_CanBeCaughtAsException()
    {
        // Arrange
        var exception = new ConcurrencyException("Test exception");

        // Act & Assert
        Assert.IsAssignableFrom<Exception>(exception);
        Assert.IsType<ConcurrencyException>(exception);
    }
}
