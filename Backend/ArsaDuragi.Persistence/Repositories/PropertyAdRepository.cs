using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.Interfaces.Repositories;
using ArsaDuragi.Domain.Entities;
using ArsaDuragi.Domain.Enums;
using ArsaDuragi.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ArsaDuragi.Persistence.Repositories;

public class PropertyAdRepository : IPropertyAdRepository
{
    private readonly AppDbContext _db;

    public PropertyAdRepository(AppDbContext db) => _db = db;

    public async Task<PagedResult<PropertyAd>> SearchAsync(PropertyAdSearchCriteria criteria, CancellationToken ct = default)
    {
        var f = criteria.Filter;
        var page = Math.Max(1, f.Page);
        var pageSize = Math.Clamp(f.PageSize, 1, 50);

        // AsNoTracking: this is a read-only query, so EF skips change-tracking work.
        // Everything below is translated to SQL; nothing is filtered in memory.
        IQueryable<PropertyAd> query = _db.PropertyAds.AsNoTracking();

        var ownerId = criteria.OwnerId;
        if (ownerId is not null)
            query = query.Where(p => p.UserId == ownerId);

        var statuses = criteria.Statuses?.ToArray();
        if (statuses is { Length: > 0 })
            query = query.Where(p => statuses.Contains(p.Status));

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var pattern = $"%{EscapeLike(f.Search.Trim())}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Title, pattern) ||
                EF.Functions.Like(p.Address, pattern) ||
                EF.Functions.Like(p.Description, pattern) ||
                EF.Functions.Like(p.District.Name, pattern) ||
                EF.Functions.Like(p.City.Name, pattern));
        }

        if (f.DistrictId.HasValue) { var v = f.DistrictId.Value; query = query.Where(p => p.DistrictId == v); }
        if (f.CityId.HasValue) { var v = f.CityId.Value; query = query.Where(p => p.CityId == v); }
        if (f.PropertyType.HasValue) { var v = f.PropertyType.Value; query = query.Where(p => p.PropertyType == v); }
        if (f.Zoning.HasValue) { var v = f.Zoning.Value; query = query.Where(p => p.Zoning == v); }
        if (f.MinPrice.HasValue) { var v = f.MinPrice.Value; query = query.Where(p => p.Price >= v); }
        if (f.MaxPrice.HasValue) { var v = f.MaxPrice.Value; query = query.Where(p => p.Price <= v); }
        if (f.MinArea.HasValue) { var v = f.MinArea.Value; query = query.Where(p => p.Area >= v); }
        if (f.MaxArea.HasValue) { var v = f.MaxArea.Value; query = query.Where(p => p.Area <= v); }
        if (f.Featured.HasValue) { var v = f.Featured.Value; query = query.Where(p => p.IsFeatured == v); }

        var total = await query.CountAsync(ct);

        var desc = !string.Equals(f.SortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        IOrderedQueryable<PropertyAd> ordered = (f.SortBy ?? "listedat").ToLowerInvariant() switch
        {
            "price" => desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "area" => desc ? query.OrderByDescending(p => p.Area) : query.OrderBy(p => p.Area),
            "featured" => desc
                ? query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.ListedAt ?? p.CreatedAt)
                : query.OrderBy(p => p.IsFeatured).ThenBy(p => p.ListedAt ?? p.CreatedAt),
            _ => desc ? query.OrderByDescending(p => p.ListedAt ?? p.CreatedAt) : query.OrderBy(p => p.ListedAt ?? p.CreatedAt)
        };

        // Id as the last tie-breaker keeps paging stable when many rows share the same price/area/date.
        ordered = ordered.ThenByDescending(p => p.Id);

        var items = await ordered
            .Include(p => p.City)
            .Include(p => p.District)
            .Include(p => p.Images)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<PropertyAd> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public Task<PropertyAd?> GetByIdAsync(int id, bool track = false, CancellationToken ct = default)
        => WithDetails(track).FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<PropertyAd?> GetBySlugAsync(string slug, bool track = false, CancellationToken ct = default)
        => WithDetails(track).FirstOrDefaultAsync(p => p.Slug == slug, ct);

    public async Task<IReadOnlyList<PropertyAd>> GetFeaturedAsync(int take, CancellationToken ct = default)
        => await _db.PropertyAds.AsNoTracking()
            .Where(p => p.Status == PropertyStatus.Active && p.IsFeatured)
            .OrderByDescending(p => p.ListedAt ?? p.CreatedAt).ThenByDescending(p => p.Id)
            .Include(p => p.City).Include(p => p.District).Include(p => p.Images)
            .Take(take)
            .ToListAsync(ct);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default)
        => _db.PropertyAds.IgnoreQueryFilters().AnyAsync(p => p.Slug == slug, ct);

    public async Task AddAsync(PropertyAd property, CancellationToken ct = default)
        => await _db.PropertyAds.AddAsync(property, ct);

    private IQueryable<PropertyAd> WithDetails(bool track)
    {
        IQueryable<PropertyAd> query = _db.PropertyAds
            .Include(p => p.City)
            .Include(p => p.District)
            .Include(p => p.User)
            .Include(p => p.Images.OrderByDescending(i => i.IsMain).ThenBy(i => i.Id));

        return track ? query : query.AsNoTracking();
    }

    /// <summary>Escapes characters that have a special meaning inside SQL LIKE patterns.</summary>
    private static string EscapeLike(string value)
        => value.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
}
