using ArsaDuragi.Application.DTOs.PropertyImage;

namespace ArsaDuragi.Application.DTOs.PropertyAd;

/// <summary>Full listing, consumed by the frontend's PropertyDetailsPage.</summary>
public class PropertyAdGetDto : PropertyAdListDto
{
    public string Description { get; set; } = string.Empty;

    public string Terrain { get; set; } = string.Empty;
    public string TerrainCode { get; set; } = string.Empty;

    /// <summary>Display label, e.g. "Ana yola 300 m".</summary>
    public string RoadAccess { get; set; } = string.Empty;
    public string RoadAccessCode { get; set; } = string.Empty;
    public int? RoadDistanceMeters { get; set; }

    /// <summary>Display labels, e.g. ["Elektrik", "Su"]. The details page maps over this array.</summary>
    public List<string> Utilities { get; set; } = new();
    public List<string> UtilityCodes { get; set; } = new();

    public List<PropertyImageDto> Images { get; set; } = new();

    public string OwnerName { get; set; } = string.Empty;
    public string? OwnerPhone { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
