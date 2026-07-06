namespace Application.Common.Exceptions;

public class NotFoundException : ApiException
{
    public NotFoundException(string message)
        : base(message, 404)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.", 404)
    {
    }
}