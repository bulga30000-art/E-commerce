namespace E_commerce.Exceptions;

// Represents Bad Request (400) errors such as invalid payload or domain rule violations
public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(message, StatusCodes.Status400BadRequest) { }
}