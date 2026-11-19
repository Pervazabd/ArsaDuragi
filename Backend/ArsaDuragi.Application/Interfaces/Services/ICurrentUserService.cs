namespace ArsaDuragi.Application.Interfaces.Services;

/// <summary>Who is calling? Implemented in the API layer from the JWT claims.</summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}
