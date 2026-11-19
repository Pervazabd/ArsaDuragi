using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.PropertyAd;
using ArsaDuragi.Domain.Enums;

namespace ArsaDuragi.Application.Interfaces.Services;

public interface IPropertyAdService
{
    // Public
    Task<PagedResult<PropertyAdListDto>> GetPublicAsync(PropertyAdFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<PropertyAdListDto>> GetFeaturedAsync(int take, CancellationToken ct = default);
    Task<PropertyAdGetDto> GetByIdOrSlugAsync(string idOrSlug, CancellationToken ct = default);

    // Authenticated user
    Task<PagedResult<PropertyAdListDto>> GetMineAsync(PropertyAdFilterDto filter, PropertyStatus? status, CancellationToken ct = default);
    Task<PropertyAdGetDto> CreateAsync(PropertyAdCreateDto dto, CancellationToken ct = default);
    Task<PropertyAdGetDto> UpdateAsync(int id, PropertyAdUpdateDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<PropertyAdGetDto> ChangeStatusAsync(int id, PropertyAdStatusChangeDto dto, CancellationToken ct = default);

    // Admin only
    Task<PagedResult<PropertyAdListDto>> GetAllForAdminAsync(PropertyAdFilterDto filter, PropertyStatus? status, CancellationToken ct = default);
    Task<PropertyAdGetDto> ApproveAsync(int id, CancellationToken ct = default);
    Task<PropertyAdGetDto> RejectAsync(int id, RejectPropertyAdDto dto, CancellationToken ct = default);
    Task<PropertyAdGetDto> SetFeaturedAsync(int id, bool isFeatured, CancellationToken ct = default);
}
