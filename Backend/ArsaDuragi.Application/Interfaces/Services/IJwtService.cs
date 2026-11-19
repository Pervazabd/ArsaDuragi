using ArsaDuragi.Application.Common;
using ArsaDuragi.Domain.Entities;

namespace ArsaDuragi.Application.Interfaces.Services;

public interface IJwtService
{
    AccessTokenResult CreateAccessToken(User user, IEnumerable<string> roles);

    /// <summary>Creates a new random refresh token. Only <see cref="GeneratedRefreshToken.TokenHash"/> may be stored.</summary>
    GeneratedRefreshToken CreateRefreshToken();

    string HashToken(string rawToken);
}
