namespace ArsaDuragi.Application.Interfaces.Services;

public interface IRegistrationEmailToken
{
    string? Create(string email);
    bool IsValid(string email, string token);
}