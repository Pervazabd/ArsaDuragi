namespace ArsaDuragi.Domain.Entities;

/// <summary>
/// Only the SHA-256 hash of the token is stored. A database leak therefore does not leak usable tokens.
/// </summary>
public class RefreshToken : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTime.UtcNow;
}
