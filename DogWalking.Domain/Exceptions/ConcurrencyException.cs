namespace DogWalking.Domain.Exceptions;

/// <summary>
/// Wraps EF Core's DbUpdateConcurrencyException with a user-friendly message.
/// Triggered when two users try to modify the same record at the same time.
/// </summary>
public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message, Exception inner) : base(message, inner) { }
}
