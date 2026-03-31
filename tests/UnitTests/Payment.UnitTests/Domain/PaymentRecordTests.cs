using FluentAssertions;
using NovaCart.Services.Payment.Domain.Entities;
using NovaCart.Services.Payment.Domain.ValueObjects;

namespace NovaCart.Tests.Payment.UnitTests.Domain;

public class PaymentRecordTests
{
    [Fact]
    public void Create_Should_ReturnPaymentRecord_When_ValidData()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var payment = PaymentRecord.Create(orderId, 999.99m, "USD");

        // Assert
        payment.Should().NotBeNull();
        payment.OrderId.Should().Be(orderId);
        payment.Amount.Should().Be(999.99m);
        payment.Currency.Should().Be("USD");
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.ProcessedAt.Should().BeNull();
        payment.FailureReason.Should().BeNull();
        payment.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_Should_NormalizeCurrency_When_LowerCase()
    {
        // Act
        var payment = PaymentRecord.Create(Guid.NewGuid(), 10m, "usd");

        // Assert
        payment.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_Should_ThrowArgumentException_When_EmptyOrderId()
    {
        // Act
        var act = () => PaymentRecord.Create(Guid.Empty, 10m, "USD");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("orderId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999.99)]
    public void Create_Should_ThrowArgumentException_When_InvalidAmount(decimal amount)
    {
        // Act
        var act = () => PaymentRecord.Create(Guid.NewGuid(), amount, "USD");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("amount");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowArgumentException_When_EmptyCurrency(string? currency)
    {
        // Act
        var act = () => PaymentRecord.Create(Guid.NewGuid(), 10m, currency!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("currency");
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDT")]
    [InlineData("12A")]
    public void Create_Should_ThrowArgumentException_When_InvalidCurrencyFormat(string currency)
    {
        // Act
        var act = () => PaymentRecord.Create(Guid.NewGuid(), 10m, currency);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("currency");
    }

    [Fact]
    public void MarkAsSucceeded_Should_UpdateStatus_When_Pending()
    {
        // Arrange
        var payment = PaymentRecord.Create(Guid.NewGuid(), 999.99m, "USD");

        // Act
        payment.MarkAsSucceeded();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsSucceeded_Should_ThrowInvalidOperationException_When_AlreadySucceeded()
    {
        // Arrange
        var payment = PaymentRecord.Create(Guid.NewGuid(), 999.99m, "USD");
        payment.MarkAsSucceeded();

        // Act
        var act = () => payment.MarkAsSucceeded();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkAsSucceeded_Should_ThrowInvalidOperationException_When_AlreadyFailed()
    {
        // Arrange
        var payment = PaymentRecord.Create(Guid.NewGuid(), 999.99m, "USD");
        payment.MarkAsFailed("Some reason");

        // Act
        var act = () => payment.MarkAsSucceeded();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkAsFailed_Should_UpdateStatusAndReason_When_Pending()
    {
        // Arrange
        var payment = PaymentRecord.Create(Guid.NewGuid(), 999.99m, "USD");
        var reason = "Insufficient funds";

        // Act
        payment.MarkAsFailed(reason);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be(reason);
        payment.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsFailed_Should_ThrowInvalidOperationException_When_AlreadySucceeded()
    {
        // Arrange
        var payment = PaymentRecord.Create(Guid.NewGuid(), 999.99m, "USD");
        payment.MarkAsSucceeded();

        // Act
        var act = () => payment.MarkAsFailed("Some reason");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MarkAsFailed_Should_ThrowArgumentException_When_EmptyReason(string? reason)
    {
        // Arrange
        var payment = PaymentRecord.Create(Guid.NewGuid(), 999.99m, "USD");

        // Act
        var act = () => payment.MarkAsFailed(reason!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("reason");
    }

    [Fact]
    public void MarkAsFailed_Should_ThrowArgumentException_When_ReasonTooLong()
    {
        // Arrange
        var payment = PaymentRecord.Create(Guid.NewGuid(), 999.99m, "USD");
        var reason = new string('x', 501);

        // Act
        var act = () => payment.MarkAsFailed(reason);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("reason");
    }
}
