namespace ArsaDuragi.Application.Interfaces.Services;

public interface IPasswordResetCode
{
    string? Create(string userId, string identityToken);
    string? GetIdentityToken(string userId, string code);
}