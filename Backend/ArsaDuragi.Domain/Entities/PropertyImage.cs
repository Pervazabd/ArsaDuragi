namespace ArsaDuragi.Domain.Entities;

public class PropertyImage : BaseEntity
{
    public int PropertyAdId { get; set; }
    public PropertyAd PropertyAd { get; set; } = null!;

    /// <summary>Relative URL for locally stored files (/uploads/properties/...) or an absolute URL for demo images.</summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Storage key (properties/{propertyId}/{guid}.jpg). Null for external demo images, which are never deleted from disk.</summary>
    public string? ObjectKey { get; set; }

    public bool IsMain { get; set; }
}
