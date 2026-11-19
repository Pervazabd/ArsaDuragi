namespace ArsaDuragi.Application.Exceptions;

/// <summary>Maps to HTTP 409 (duplicate email, duplicate favorite, city still has districts...).</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
