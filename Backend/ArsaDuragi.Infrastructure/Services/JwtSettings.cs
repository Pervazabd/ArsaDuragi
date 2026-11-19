namespace ArsaDuragi.Infrastructure.Services;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>Signing key. NEVER put it in appsettings.json: use user-secrets (dev) or an environment variable (prod).</summary>
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "ArsaDuragi";
    public string Audience { get; set; } = "ArsaDuragi.Frontend";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;

    /// <summary>Fails fast at startup with a readable message instead of a confusing runtime error later.</summary>
    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(Secret) || Secret.Length < 32)
            throw new InvalidOperationException(
                "Jwt:Secret is missing or shorter than 32 characters. " +
                "Set it with: dotnet user-secrets set \"Jwt:Secret\" \"<at least 32 random characters>\" " +
                "(or the environment variable Jwt__Secret).");
    }
}
