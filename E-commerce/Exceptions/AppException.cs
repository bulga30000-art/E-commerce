namespace E_commerce.Exceptions;

// Base abstract exception class for custom application domain exceptions
public abstract class AppException : Exception
{
    // The HTTP status code to return in the error response
    public int StatusCode { get; }

    protected AppException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}