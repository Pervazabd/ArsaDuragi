namespace ArsaDuragi.Application.Interfaces.Services;

/// <summary>
/// Proof that request-verification was called for one normalized Gmail address.
/// Uses its own ASP.NET Core Data Protection purpose, separate from Identity password-reset tokens.
/// </summary>
public interface IRegistrationVerificationTokenService
{
    string Create(string normalizedEmail);
    bool TryValidate(string? token, string normalizedEmail);
}
