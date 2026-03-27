namespace NovaCart.BuildingBlocks.Common.Exceptions;

public sealed class ConflictException : Exception
{
    public ConflictException(string entityName, object id)
        : base($"{entityName} with id '{id}' already exists.")
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }
}
