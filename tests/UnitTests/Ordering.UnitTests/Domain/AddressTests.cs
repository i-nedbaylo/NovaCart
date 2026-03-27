using FluentAssertions;
using NovaCart.Services.Ordering.Domain.ValueObjects;

namespace NovaCart.Tests.Ordering.UnitTests.Domain;

public class AddressTests
{
    [Fact]
    public void Create_Should_ReturnAddress_When_ValidData()
    {
        // Act
        var address = Address.Create("123 Main St", "Springfield", "IL", "US", "62704");

        // Assert
        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("Springfield");
        address.State.Should().Be("IL");
        address.Country.Should().Be("US");
        address.ZipCode.Should().Be("62704");
    }

    [Theory]
    [InlineData("", "City", "State", "Country", "12345")]
    [InlineData("Street", "", "State", "Country", "12345")]
    [InlineData("Street", "City", "", "Country", "12345")]
    [InlineData("Street", "City", "State", "", "12345")]
    [InlineData("Street", "City", "State", "Country", "")]
    public void Create_Should_ThrowArgumentException_When_AnyFieldIsEmpty(
        string street, string city, string state, string country, string zipCode)
    {
        // Act
        var act = () => Address.Create(street, city, state, country, zipCode);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_Should_BeTrue_When_SameValues()
    {
        // Arrange
        var address1 = Address.Create("123 Main St", "Springfield", "IL", "US", "62704");
        var address2 = Address.Create("123 Main St", "Springfield", "IL", "US", "62704");

        // Assert
        address1.Should().Be(address2);
    }

    [Fact]
    public void Equality_Should_BeFalse_When_DifferentValues()
    {
        // Arrange
        var address1 = Address.Create("123 Main St", "Springfield", "IL", "US", "62704");
        var address2 = Address.Create("456 Oak Ave", "Springfield", "IL", "US", "62704");

        // Assert
        address1.Should().NotBe(address2);
    }
}
