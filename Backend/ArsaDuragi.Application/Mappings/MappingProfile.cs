using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.Auth;
using ArsaDuragi.Application.DTOs.Favorite;
using ArsaDuragi.Application.DTOs.Location;
using ArsaDuragi.Application.DTOs.PropertyAd;
using ArsaDuragi.Application.DTOs.PropertyImage;
using ArsaDuragi.Domain.Entities;
using ArsaDuragi.Domain.Enums;
using AutoMapper;

namespace ArsaDuragi.Application.Mappings;

/// <summary>
/// Entity -> DTO mappings (read side only). Writing (DTO -> entity) is done by hand in the services on purpose:
/// it keeps over-posting impossible, because only fields we copy explicitly can change.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PropertyImage, PropertyImageDto>()
            .ForMember(d => d.Url, o => o.MapFrom(s => s.ImageUrl));

        CreateMap<PropertyAd, PropertyAdListDto>()
            .ForMember(d => d.City, o => o.MapFrom(s => s.City.Name))
            .ForMember(d => d.District, o => o.MapFrom(s => s.District.Name))
            .ForMember(d => d.Type, o => o.MapFrom(s => PropertyLabels.Type(s.PropertyType)))
            .ForMember(d => d.TypeCode, o => o.MapFrom(s => s.PropertyType.ToString()))
            .ForMember(d => d.Zoning, o => o.MapFrom(s => PropertyLabels.Zoning(s.Zoning)))
            .ForMember(d => d.ZoningCode, o => o.MapFrom(s => s.Zoning.ToString()))
            .ForMember(d => d.PricePerSquareMeter, o => o.MapFrom(s => s.Area > 0 ? Math.Round(s.Price / s.Area, 0) : 0m))
            .ForMember(d => d.Featured, o => o.MapFrom(s => s.IsFeatured))
            .ForMember(d => d.Image, o => o.MapFrom(s => MainImageUrl(s)))
            .ForMember(d => d.Transaction, o => o.Ignore())
            .ForMember(d => d.Verified, o => o.MapFrom(s => s.Status == PropertyStatus.Active && !s.IsDemo))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<PropertyAd, PropertyAdGetDto>()
            .IncludeBase<PropertyAd, PropertyAdListDto>()
            .ForMember(d => d.Terrain, o => o.MapFrom(s => PropertyLabels.Terrain(s.Terrain)))
            .ForMember(d => d.TerrainCode, o => o.MapFrom(s => s.Terrain.ToString()))
            .ForMember(d => d.RoadAccess, o => o.MapFrom(s => PropertyLabels.RoadAccess(s.RoadAccess, s.RoadDistanceMeters)))
            .ForMember(d => d.RoadAccessCode, o => o.MapFrom(s => s.RoadAccess.ToString()))
            .ForMember(d => d.Utilities, o => o.MapFrom(s => PropertyLabels.UtilityLabels(s.Utilities)))
            .ForMember(d => d.UtilityCodes, o => o.MapFrom(s => PropertyLabels.UtilityCodes(s.Utilities)))
            .ForMember(d => d.Images, o => o.MapFrom(s => s.Images.OrderByDescending(i => i.IsMain).ThenBy(i => i.Id)))
            .ForMember(d => d.OwnerName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.OwnerPhone, o => o.MapFrom(s => s.User.PhoneNumber));

        CreateMap<Favorite, FavoriteDto>()
            .ForMember(d => d.Property, o => o.MapFrom(s => s.PropertyAd));

        CreateMap<City, CityDto>();
        CreateMap<District, DistrictDto>()
            .ForMember(d => d.CityName, o => o.MapFrom(s => s.City.Name));

        CreateMap<User, UserDto>()
            .ForMember(d => d.Roles, o => o.Ignore())
            .ForMember(d => d.IsLockedOut, o => o.MapFrom(s => s.LockoutEnd.HasValue && s.LockoutEnd.Value > DateTimeOffset.UtcNow));
    }

    private static string? MainImageUrl(PropertyAd ad)
        => ad.Images.OrderByDescending(i => i.IsMain).ThenBy(i => i.Id).Select(i => i.ImageUrl).FirstOrDefault();
}
