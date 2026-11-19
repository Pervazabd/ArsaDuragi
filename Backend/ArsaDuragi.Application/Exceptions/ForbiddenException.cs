namespace ArsaDuragi.Application.Exceptions;

/// <summary>Maps to HTTP 403: the user is known but not allowed to do this.</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
