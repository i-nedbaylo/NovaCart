namespace NovaCart.BuildingBlocks.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Failure
}

public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");

    public static Error NotFound(string entityName, object id) =>
        new($"{entityName}.NotFound", $"{entityName} with id '{id}' was not found.", ErrorType.NotFound);

    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation);

    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);
}
