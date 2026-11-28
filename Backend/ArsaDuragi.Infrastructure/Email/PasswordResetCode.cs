using ArsaDuragi.Application.Interfaces.Services;

namespace ArsaDuragi.Infrastructure.Email;

internal sealed class PasswordResetCode(EmailVerificationCodes codes)
    : IPasswordResetCode
{
    public string? Create(string userId, string identityToken)
        => codes.Create(typeof(PasswordResetCode), userId, identityToken);

    public string? GetIdentityToken(string userId, string code)
        => codes.Verify(typeof(PasswordResetCode), userId, code);
}