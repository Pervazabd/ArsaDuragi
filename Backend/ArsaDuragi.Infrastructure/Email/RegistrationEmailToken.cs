using ArsaDuragi.Application.Interfaces.Services;

namespace ArsaDuragi.Infrastructure.Email;

internal sealed class RegistrationEmailToken(EmailVerificationCodes codes)
    : IRegistrationEmailToken
{
    public string? Create(string email)
        => codes.Create(typeof(RegistrationEmailToken), email, email);

    public bool IsValid(string email, string token)
        => codes.Verify(typeof(RegistrationEmailToken), email, token) is not null;
}