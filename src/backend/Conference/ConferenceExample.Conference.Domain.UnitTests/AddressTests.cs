using ConferenceExample.Conference.Domain.ConferenceManagement;

namespace ConferenceExample.Conference.Domain.UnitTests;

public class AddressTests
{
    [Fact]
    public void Constructor_ValidParameters_InitializesProperties()
    {
        // Arrange
        var street = "123 Main St";
        var city = "Springfield";
        var state = "IL";
        var postalCode = "62701";
        var country = "USA";

        // Act
        var address = new Address(street, city, state, postalCode, country);

        // Assert
        Assert.Equal(street, address.Street);
        Assert.Equal(city, address.City);
        Assert.Equal(state, address.State);
        Assert.Equal(postalCode, address.PostalCode);
        Assert.Equal(country, address.Country);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Springfield", "IL", "62701", "USA");
        var address2 = new Address("123 Main St", "Springfield", "IL", "62701", "USA");

        // Act & Assert
        Assert.Equal(address1, address2);
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Springfield", "IL", "62701", "USA");
        var address2 = new Address("456 Oak Ave", "Springfield", "IL", "62701", "USA");

        // Act & Assert
        Assert.NotEqual(address1, address2);
    }
}
