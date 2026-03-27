namespace NovaCart.BuildingBlocks.Common.Exceptions;

public sealed class AppValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public AppValidationException(IEnumerable<string> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.ToList().AsReadOnly();
    }

    public AppValidationException(string error)
        : base(error)
    {
        Errors = [error];
    }
}
