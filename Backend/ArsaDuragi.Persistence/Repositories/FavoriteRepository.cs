using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.Interfaces.Repositories;
using ArsaDuragi.Domain.Entities;
using ArsaDuragi.Domain.Enums;
using ArsaDuragi.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ArsaDuragi.Persistence.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly AppDbContext _db;

    public FavoriteRepository(AppDbContext db) => _db = db;

    public Task<bool> ExistsAsync(string userId, int propertyAdId, CancellationToken ct = default)
        => _db.Favorites.AnyAsync(f => f.UserId == userId && f.PropertyAdId == propertyAdId, ct);

    public Task<Favorite?> GetAsync(string userId, int propertyAdId, CancellationToken ct = default)
        => _db.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.PropertyAdId == propertyAdId, ct);

    public async Task AddAsync(Favorite favorite, CancellationToken ct = default)
        => await _db.Favorites.AddAsync(favorite, ct);

    public void Remove(Favorite favorite) => _db.Favorites.Remove(favorite);

    public async Task<PagedResult<Favorite>> GetByUserAsync(string userId, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _db.Favorites.AsNoTracking()
            .Where(f => f.UserId == userId && f.PropertyAd.Status == PropertyStatus.Active);

        var total = await query.CountAsync(ct);
        var items = await query
            .Include(f => f.PropertyAd).ThenInclude(p => p.City)
            .Include(f => f.PropertyAd).ThenInclude(p => p.District)
            .Include(f => f.PropertyAd).ThenInclude(p => p.Images)
            .OrderByDescending(f => f.CreatedAt).ThenByDescending(f => f.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Favorite> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<IReadOnlyList<int>> GetPropertyIdsAsync(string userId, CancellationToken ct = default)
        => await _db.Favorites.AsNoTracking()
            .Where(f => f.UserId == userId && f.PropertyAd.Status == PropertyStatus.Active)
            .Select(f => f.PropertyAdId).ToListAsync(ct);
}
