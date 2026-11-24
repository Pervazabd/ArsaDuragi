using System.Security.Cryptography;
using ArsaDuragi.Application.Interfaces.Services;
using Microsoft.AspNetCore.DataProtection;

namespace ArsaDuragi.Infrastructure.Email;

public sealed class RegistrationEmailToken : IRegistrationEmailToken
{
    private readonly ITimeLimitedDataProtector _protector;

    public RegistrationEmailToken(IDataProtectionProvider provider)
    {
        _protector = provider
            .CreateProtector("ArsaDuragi.RegistrationEmail.v1")
            .ToTimeLimitedDataProtector();
    }

    public string Create(string email)
        => _protector.Protect(email, TimeSpan.FromMinutes(30));

    public bool IsValid(string email, string token)
    {
        try
        {
            return string.Equals(
                _protector.Unprotect(token), email, StringComparison.Ordinal);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException)
        {
            return false;
        }
    }
}