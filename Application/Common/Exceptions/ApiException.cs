namespace Application.Common.Exceptions;

public class ApiException : Exception
{
    public int StatusCode { get; }

    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    public ApiException(
        string message,
        int statusCode = 500,
        IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}