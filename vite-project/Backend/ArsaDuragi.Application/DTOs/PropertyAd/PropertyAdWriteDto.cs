using ArsaDuragi.Domain.Enums;

namespace ArsaDuragi.Application.DTOs.PropertyAd;

/// <summary>
/// Fields a user may send when creating or editing a listing.
/// Deliberately NOT included (to prevent over-posting): UserId, Status, IsFeatured, ListedAt, IsDemo, Slug.
/// </summary>
public abstract class PropertyAdWriteDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Area { get; set; }
    public PropertyType PropertyType { get; set; }
    public PropertyZoning Zoning { get; set; }
    public TerrainType Terrain { get; set; }
    public RoadAccessType RoadAccess { get; set; }

    /// <summary>Required only when RoadAccess is MainRoadNearby or AsphaltRoadNearby.</summary>
    public int? RoadDistanceMeters { get; set; }

    public List<UtilityType> Utilities { get; set; } = new();
    public int CityId { get; set; }
    public int DistrictId { get; set; }
}
