namespace DogWalking.Domain.Exceptions;

/// <summary>
/// Thrown when a business rule is violated (invalid transition, limit exceeded, etc.).
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
