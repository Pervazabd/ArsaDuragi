namespace ArsaDuragi.Application.Exceptions;

/// <summary>Maps to HTTP 401: missing/invalid credentials or tokens.</summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}
