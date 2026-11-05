using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.Interfaces.Services;
using ArsaDuragi.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ArsaDuragi.Infrastructure.Services;

public sealed class JwtService : IJwtService
{
    private readonly JwtSettings _settings;

    public JwtService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
        _settings.EnsureValid();
    }

    public AccessTokenResult CreateAccessToken(User user, IEnumerable<string> roles)
    {
        // Short claim names ("sub", "role"...) are used on purpose; the API reads them back with the same names.
        var claims = new List<Claim>
        {
            new("sub", user.Id),
            new("email", user.Email ?? string.Empty),
            new("name", user.FullName),
            new("jti", Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim("role", r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: credentials);

        return new AccessTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public GeneratedRefreshToken CreateRefreshToken()
    {
        // 64 cryptographically-random bytes; the raw value goes to the client, only its hash is stored.
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        return new GeneratedRefreshToken(raw, HashToken(raw), DateTime.UtcNow.AddDays(_settings.RefreshTokenDays));
    }

    public string HashToken(string rawToken)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
