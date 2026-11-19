namespace ArsaDuragi.Application.Exceptions;

/// <summary>Maps to HTTP 400. Optionally carries a list of detailed errors.</summary>
public class BadRequestException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public BadRequestException(string message, IEnumerable<string>? errors = null) : base(message)
    {
        Errors = errors?.ToList() ?? new List<string>();
    }
}
