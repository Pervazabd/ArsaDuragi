using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.Favorite;

namespace ArsaDuragi.Application.Interfaces.Services;

public interface IFavoriteService
{
    Task<FavoriteDto> AddAsync(int propertyId, CancellationToken ct = default);
    Task RemoveAsync(int propertyId, CancellationToken ct = default);
    Task<PagedResult<FavoriteDto>> GetMineAsync(int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<int>> GetMyPropertyIdsAsync(CancellationToken ct = default);
}
