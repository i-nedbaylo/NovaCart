using FluentAssertions;
using NovaCart.Services.Payment.Domain.ValueObjects;

namespace NovaCart.Tests.Payment.UnitTests.Domain;

public class PaymentStatusTests
{
    [Theory]
    [InlineData("Pending")]
    [InlineData("Succeeded")]
    [InlineData("Failed")]
    public void From_Should_ReturnStatus_When_ValidValue(string value)
    {
        // Act
        var status = PaymentStatus.From(value);

        // Assert
        status.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("pending")]
    [InlineData("Invalid")]
    [InlineData("")]
    public void From_Should_ThrowArgumentException_When_InvalidValue(string value)
    {
        // Act
        var act = () => PaymentStatus.From(value);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_Should_BeTrue_When_SameStatus()
    {
        // Assert
        PaymentStatus.Pending.Should().Be(PaymentStatus.Pending);
        PaymentStatus.Succeeded.Should().Be(PaymentStatus.Succeeded);
        PaymentStatus.Failed.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public void Equality_Should_BeFalse_When_DifferentStatus()
    {
        // Assert
        PaymentStatus.Pending.Should().NotBe(PaymentStatus.Succeeded);
        PaymentStatus.Succeeded.Should().NotBe(PaymentStatus.Failed);
    }

    [Fact]
    public void ImplicitConversion_Should_ReturnStringValue()
    {
        // Arrange
        string value = PaymentStatus.Pending;

        // Assert
        value.Should().Be("Pending");
    }

    [Fact]
    public void ToString_Should_ReturnValue()
    {
        // Assert
        PaymentStatus.Pending.ToString().Should().Be("Pending");
        PaymentStatus.Succeeded.ToString().Should().Be("Succeeded");
        PaymentStatus.Failed.ToString().Should().Be("Failed");
    }
}
