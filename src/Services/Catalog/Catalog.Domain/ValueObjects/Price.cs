namespace NovaCart.Services.Catalog.Domain.ValueObjects;

public sealed record Price
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Price() { Currency = "USD"; }

    private Price(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Price Create(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("Price amount cannot be negative.", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));

        return new Price(amount, currency.ToUpperInvariant());
    }
}
