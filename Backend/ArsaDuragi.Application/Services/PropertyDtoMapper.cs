using ArsaDuragi.Application.DTOs.Favorite;
using ArsaDuragi.Application.DTOs.PropertyAd;
using ArsaDuragi.Application.DTOs.PropertyImage;
using ArsaDuragi.Application.Interfaces.Storage;
using ArsaDuragi.Domain.Entities;
using AutoMapper;

namespace ArsaDuragi.Application.Services;

/// <summary>
/// AutoMapper does the structural mapping; this small wrapper then converts stored relative image paths
/// into URLs the browser can load (needs IFileStorageService, which AutoMapper itself should not depend on).
/// </summary>
public sealed class PropertyDtoMapper
{
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;

    public PropertyDtoMapper(IMapper mapper, IFileStorageService storage)
    {
        _mapper = mapper;
        _storage = storage;
    }

    public PropertyAdListDto ToList(PropertyAd property)
    {
        var dto = _mapper.Map<PropertyAdListDto>(property);
        dto.Image = _storage.GetPublicUrl(dto.Image);
        return dto;
    }

    public PropertyAdGetDto ToDetails(PropertyAd property)
    {
        var dto = _mapper.Map<PropertyAdGetDto>(property);
        dto.Image = _storage.GetPublicUrl(dto.Image);
        foreach (var image in dto.Images)
            image.Url = _storage.GetPublicUrl(image.Url) ?? string.Empty;
        return dto;
    }

    public PropertyImageDto ToImage(PropertyImage image)
    {
        var dto = _mapper.Map<PropertyImageDto>(image);
        dto.Url = _storage.GetPublicUrl(dto.Url) ?? string.Empty;
        return dto;
    }

    public FavoriteDto ToFavorite(Favorite favorite)
    {
        var dto = _mapper.Map<FavoriteDto>(favorite);
        dto.Property.Image = _storage.GetPublicUrl(dto.Property.Image);
        return dto;
    }
}
