namespace E_commerce.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, StatusCodes.Status404NotFound) { }

    // Helper constructor to construct standardized Not Found exception messages by entity name and primary key
    public NotFoundException(string entityName, object key)
        : base($"{entityName} برقم '{key}' غير موجود", StatusCodes.Status404NotFound) { }
}