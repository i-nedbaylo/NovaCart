namespace NovaCart.Services.Payment.Domain.ValueObjects;

public sealed class PaymentStatus
{
    public static readonly PaymentStatus Pending = new("Pending");
    public static readonly PaymentStatus Succeeded = new("Succeeded");
    public static readonly PaymentStatus Failed = new("Failed");

    public string Value { get; }

    private PaymentStatus(string value) => Value = value;

    public static PaymentStatus From(string value)
    {
        return value switch
        {
            "Pending" => Pending,
            "Succeeded" => Succeeded,
            "Failed" => Failed,
            _ => throw new ArgumentException($"Invalid payment status: {value}", nameof(value))
        };
    }

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is PaymentStatus other && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();

    public static implicit operator string(PaymentStatus status) => status.Value;
}
