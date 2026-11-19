using ArsaDuragi.Domain.Common;
using ArsaDuragi.Domain.Enums;

namespace ArsaDuragi.Domain.Entities;

public class PropertyAd : BaseEntity, ISoftDeletable
{
    /// <summary>URL-friendly unique identifier. The seeded demo listings keep the ids the old frontend used.</summary>
    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    /// <summary>Price in AZN.</summary>
    public decimal Price { get; set; }

    /// <summary>Area in square metres.</summary>
    public decimal Area { get; set; }

    public PropertyType PropertyType { get; set; }
    public PropertyZoning Zoning { get; set; }
    public TerrainType Terrain { get; set; }
    public RoadAccessType RoadAccess { get; set; }

    /// <summary>Only used for RoadAccessType.MainRoadNearby / AsphaltRoadNearby ("Ana yola 300 m").</summary>
    public int? RoadDistanceMeters { get; set; }

    public UtilityType Utilities { get; set; }

    /// <summary>Only an Admin can change this.</summary>
    public bool IsFeatured { get; set; }

    public PropertyStatus Status { get; set; } = PropertyStatus.Pending;
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedById { get; set; }

    /// <summary>Date the listing first became publicly visible. Used by the "newest" sort.</summary>
    public DateTime? ListedAt { get; set; }

    /// <summary>True for the sample listings copied from the frontend's static file.</summary>
    public bool IsDemo { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public int CityId { get; set; }
    public City City { get; set; } = null!;

    public int DistrictId { get; set; }
    public District District { get; set; } = null!;

    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
