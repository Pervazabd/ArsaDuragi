using ArsaDuragi.Application.DTOs.PropertyAd;

namespace ArsaDuragi.Application.DTOs.Favorite;

public class FavoriteDto
{
    public int Id { get; set; }
    public int PropertyAdId { get; set; }
    public DateTime CreatedAt { get; set; }
    public PropertyAdListDto Property { get; set; } = null!;
}
