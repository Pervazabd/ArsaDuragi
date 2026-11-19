using Microsoft.AspNetCore.Identity;

namespace ArsaDuragi.Domain.Entities;

/// <summary>
/// Application user. Email, PhoneNumber, PasswordHash, lockout etc. come from IdentityUser.
/// </summary>
public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PropertyAd> PropertyAds { get; set; } = new List<PropertyAd>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
