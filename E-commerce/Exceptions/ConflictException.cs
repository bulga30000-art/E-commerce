namespace E_commerce.Exceptions;

// Represents Conflict (409) errors such as duplicate entity creation or state machine conflicts
public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, StatusCodes.Status409Conflict) { }
}