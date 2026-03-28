using NovaCart.BuildingBlocks.Common;
using NovaCart.Services.Payment.Domain.ValueObjects;

namespace NovaCart.Services.Payment.Domain.Entities;

public sealed class PaymentRecord : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public PaymentStatus Status { get; private set; } = null!;
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? FailureReason { get; private set; }

    private PaymentRecord() { }

    public static PaymentRecord Create(Guid orderId, decimal amount, string currency)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("Order ID cannot be empty.", nameof(orderId));

        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));

        var normalizedCurrency = currency.Trim().ToUpperInvariant();
        if (normalizedCurrency.Length != 3 || !normalizedCurrency.All(c => c >= 'A' && c <= 'Z'))
            throw new ArgumentException("Currency must be a 3-letter ISO 4217 code.", nameof(currency));

        return new PaymentRecord
        {
            OrderId = orderId,
            Amount = amount,
            Currency = normalizedCurrency,
            Status = PaymentStatus.Pending
        };
    }

    public void MarkAsSucceeded()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot mark payment as succeeded when status is '{Status}'.");

        Status = PaymentStatus.Succeeded;
        ProcessedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsFailed(string reason)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot mark payment as failed when status is '{Status}'.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason cannot be empty.", nameof(reason));

        if (reason.Length > 500)
            throw new ArgumentException("Failure reason cannot exceed 500 characters.", nameof(reason));

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        ProcessedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
