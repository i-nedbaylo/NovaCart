namespace NovaCart.BuildingBlocks.Common;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
