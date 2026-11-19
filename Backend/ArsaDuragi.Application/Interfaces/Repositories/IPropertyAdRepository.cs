using ArsaDuragi.Application.Common;
using ArsaDuragi.Domain.Entities;

namespace ArsaDuragi.Application.Interfaces.Repositories;

public interface IPropertyAdRepository
{
    Task<PagedResult<PropertyAd>> SearchAsync(PropertyAdSearchCriteria criteria, CancellationToken ct = default);

    /// <summary>Loads the listing with City, District, User and Images. Soft-deleted listings are not returned.</summary>
    Task<PropertyAd?> GetByIdAsync(int id, bool track = false, CancellationToken ct = default);

    Task<PropertyAd?> GetBySlugAsync(string slug, bool track = false, CancellationToken ct = default);

    Task<IReadOnlyList<PropertyAd>> GetFeaturedAsync(int take, CancellationToken ct = default);

    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);

    Task AddAsync(PropertyAd property, CancellationToken ct = default);
}
