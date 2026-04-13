namespace ConferenceExample.Talk.Domain.UnitTests;

using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects;

public class NameTests
{
    [Fact]
    public void Constructor_ValidParameters_SetsProperties()
    {
        // Act
        var name = new Name("Alice", "Johnson");

        // Assert
        Assert.Equal("Alice", name.FirstName);
        Assert.Equal("Johnson", name.LastName);
    }

    [Fact]
    public void Constructor_EmptyFirstName_CreatesInstance()
    {
        // Act
        var name = new Name("", "Johnson");

        // Assert
        Assert.Equal("", name.FirstName);
        Assert.Equal("Johnson", name.LastName);
    }

    [Fact]
    public void Constructor_EmptyLastName_CreatesInstance()
    {
        // Act
        var name = new Name("Alice", "");

        // Assert
        Assert.Equal("Alice", name.FirstName);
        Assert.Equal("", name.LastName);
    }

    [Fact]
    public void Constructor_NullFirstName_CreatesInstance()
    {
        // Act
        var name = new Name(null!, "Johnson");

        // Assert
        Assert.Null(name.FirstName);
        Assert.Equal("Johnson", name.LastName);
    }

    [Fact]
    public void Constructor_NullLastName_CreatesInstance()
    {
        // Act
        var name = new Name("Alice", null!);

        // Assert
        Assert.Equal("Alice", name.FirstName);
        Assert.Null(name.LastName);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        var name1 = new Name("Bob", "Smith");
        var name2 = new Name("Bob", "Smith");

        // Act & Assert
        Assert.Equal(name1, name2);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        // Arrange
        var name1 = new Name("Bob", "Smith");
        var name2 = new Name("Alice", "Johnson");

        // Act & Assert
        Assert.NotEqual(name1, name2);
    }
}
