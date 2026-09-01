namespace ConferenceExample.Talk.Domain.UnitTests;

using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects;

public class PageRequestTests
{
    [Fact]
    public void Constructor_ValidPageAndPageSize_SetsProperties()
    {
        // Act
        var pageRequest = new PageRequest(2, 20);

        // Assert
        Assert.Equal(2, pageRequest.Page);
        Assert.Equal(20, pageRequest.PageSize);
    }

    [Fact]
    public void Skip_FirstPage_IsZero()
    {
        // Act
        var pageRequest = new PageRequest(1, 20);

        // Assert
        Assert.Equal(0, pageRequest.Skip);
    }

    [Fact]
    public void Skip_ThirdPageWithPageSizeTwenty_IsForty()
    {
        // Act
        var pageRequest = new PageRequest(3, 20);

        // Assert
        Assert.Equal(40, pageRequest.Skip);
    }

    [Fact]
    public void Constructor_PageZero_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PageRequest(0, 20));
    }

    [Fact]
    public void Constructor_NegativePage_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PageRequest(-1, 20));
    }

    [Fact]
    public void Constructor_PageSizeOne_DoesNotThrow()
    {
        // Act
        var pageRequest = new PageRequest(1, 1);

        // Assert
        Assert.Equal(1, pageRequest.PageSize);
    }

    [Fact]
    public void Constructor_PageSizeZero_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PageRequest(1, 0));
    }

    [Fact]
    public void Constructor_NegativePageSize_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PageRequest(1, -1));
    }

    [Fact]
    public void Constructor_PageSizeAtMax_DoesNotThrow()
    {
        // Act
        var pageRequest = new PageRequest(1, PageRequest.MaxPageSize);

        // Assert
        Assert.Equal(PageRequest.MaxPageSize, pageRequest.PageSize);
    }

    [Fact]
    public void Constructor_PageSizeAboveMax_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PageRequest(1, PageRequest.MaxPageSize + 1));
    }
}
