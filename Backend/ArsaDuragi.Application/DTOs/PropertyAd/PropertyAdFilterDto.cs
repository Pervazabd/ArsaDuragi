using ArsaDuragi.Domain.Enums;

namespace ArsaDuragi.Application.DTOs.PropertyAd;

/// <summary>Bound from the query string of GET /api/properties.</summary>
public class PropertyAdFilterDto
{
    public string? Search { get; set; }
    public int? DistrictId { get; set; }
    public int? CityId { get; set; }
    public PropertyType? PropertyType { get; set; }
    public PropertyZoning? Zoning { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinArea { get; set; }
    public decimal? MaxArea { get; set; }
    public bool? Featured { get; set; }

    /// <summary>listedAt (default) | price | area | featured</summary>
    public string? SortBy { get; set; }

    /// <summary>asc | desc (default desc)</summary>
    public string? SortOrder { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
