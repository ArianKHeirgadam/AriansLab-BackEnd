namespace Application.Common.Exceptions;

public class ValidationException : ApiException
{
    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Validation failed.", 400, errors)
    {
    }
}