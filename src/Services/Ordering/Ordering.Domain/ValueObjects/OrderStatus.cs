namespace NovaCart.Services.Ordering.Domain.ValueObjects;

public enum OrderStatus
{
    Created = 0,
    Confirmed = 1,
    Paid = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}
