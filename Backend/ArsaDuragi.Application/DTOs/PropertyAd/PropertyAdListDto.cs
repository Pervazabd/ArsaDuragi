namespace ArsaDuragi.Application.DTOs.PropertyAd;

/// <summary>
/// Shape consumed by the frontend's PropertyCard. Field names (id... image) intentionally match the
/// old static properties.js objects, so the card needs (almost) no changes.
/// </summary>
public class PropertyAdListDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    public int CityId { get; set; }
    public string City { get; set; } = string.Empty;
    public int DistrictId { get; set; }
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    /// <summary>Display label, e.g. "Arsa".</summary>
    public string Type { get; set; } = string.Empty;
    public string TypeCode { get; set; } = string.Empty;

    /// <summary>Display label, e.g. "Konut imarlı".</summary>
    public string Zoning { get; set; } = string.Empty;
    public string ZoningCode { get; set; } = string.Empty;

    public decimal Area { get; set; }
    public decimal Price { get; set; }
    public decimal PricePerSquareMeter { get; set; }

    public bool Featured { get; set; }
    public DateTime? ListedAt { get; set; }

    /// <summary>Absolute URL of the main image (or null if the listing has no image).</summary>
    public string? Image { get; set; }

    /// <summary>Always "Satılık" for now. PropertyCard reads property.transaction.</summary>
    public string Transaction { get; set; } = "Satılık";

    /// <summary>True when the listing was approved by an admin and is not demo data. PropertyCard reads property.verified.</summary>
    public bool Verified { get; set; }

    public bool IsDemo { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
}
