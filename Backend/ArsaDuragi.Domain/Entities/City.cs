namespace ArsaDuragi.Domain.Entities;

public class City : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<District> Districts { get; set; } = new List<District>();
    public ICollection<PropertyAd> PropertyAds { get; set; } = new List<PropertyAd>();
}
