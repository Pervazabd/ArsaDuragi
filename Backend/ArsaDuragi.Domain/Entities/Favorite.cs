namespace ArsaDuragi.Domain.Entities;

public class Favorite : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public int PropertyAdId { get; set; }
    public PropertyAd PropertyAd { get; set; } = null!;
}
