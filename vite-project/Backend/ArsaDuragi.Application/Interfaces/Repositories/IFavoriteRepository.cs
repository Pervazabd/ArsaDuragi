using ArsaDuragi.Application.Common;
using ArsaDuragi.Domain.Entities;

namespace ArsaDuragi.Application.Interfaces.Repositories;

public interface IFavoriteRepository
{
    Task<bool> ExistsAsync(string userId, int propertyAdId, CancellationToken ct = default);
    Task<Favorite?> GetAsync(string userId, int propertyAdId, CancellationToken ct = default);
    Task AddAsync(Favorite favorite, CancellationToken ct = default);
    void Remove(Favorite favorite);

    /// <summary>Only favorites whose listing is currently Active are returned.</summary>
    Task<PagedResult<Favorite>> GetByUserAsync(string userId, int page, int pageSize, CancellationToken ct = default);

    Task<IReadOnlyList<int>> GetPropertyIdsAsync(string userId, CancellationToken ct = default);
}
