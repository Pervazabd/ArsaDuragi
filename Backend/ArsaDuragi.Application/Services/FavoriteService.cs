using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.Favorite;
using ArsaDuragi.Application.Exceptions;
using ArsaDuragi.Application.Interfaces.Repositories;
using ArsaDuragi.Application.Interfaces.Services;
using ArsaDuragi.Domain.Entities;
using ArsaDuragi.Domain.Enums;

namespace ArsaDuragi.Application.Services;

public sealed class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favorites;
    private readonly IPropertyAdRepository _properties;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _current;
    private readonly PropertyDtoMapper _mapper;

    public FavoriteService(IFavoriteRepository favorites, IPropertyAdRepository properties, IUnitOfWork uow,
        ICurrentUserService current, PropertyDtoMapper mapper)
    {
        _favorites = favorites;
        _properties = properties;
        _uow = uow;
        _current = current;
        _mapper = mapper;
    }

    public async Task<FavoriteDto> AddAsync(int propertyId, CancellationToken ct = default)
    {
        var userId = OwnershipGuard.RequireUserId(_current);

        // tracked on purpose: we attach it as a navigation below, and EF must not try to INSERT it again
        var property = await _properties.GetByIdAsync(propertyId, true, ct);
        if (property is null || property.Status != PropertyStatus.Active)
            throw new NotFoundException("Property not found.");

        if (await _favorites.ExistsAsync(userId, propertyId, ct))
            throw new ConflictException("This property is already in your favorites.");

        var favorite = new Favorite { UserId = userId, PropertyAdId = propertyId, PropertyAd = property };
        await _favorites.AddAsync(favorite, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.ToFavorite(favorite);
    }

    public async Task RemoveAsync(int propertyId, CancellationToken ct = default)
    {
        var userId = OwnershipGuard.RequireUserId(_current);
        var favorite = await _favorites.GetAsync(userId, propertyId, ct) ?? throw new NotFoundException("Favorite not found.");

        _favorites.Remove(favorite);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<FavoriteDto>> GetMineAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var userId = OwnershipGuard.RequireUserId(_current);
        var result = await _favorites.GetByUserAsync(userId, page, pageSize, ct);
        return result.Map(_mapper.ToFavorite);
    }

    public Task<IReadOnlyList<int>> GetMyPropertyIdsAsync(CancellationToken ct = default)
    {
        var userId = OwnershipGuard.RequireUserId(_current);
        return _favorites.GetPropertyIdsAsync(userId, ct);
    }
}
