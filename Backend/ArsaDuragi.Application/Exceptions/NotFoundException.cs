namespace ArsaDuragi.Application.Exceptions;

/// <summary>Maps to HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
