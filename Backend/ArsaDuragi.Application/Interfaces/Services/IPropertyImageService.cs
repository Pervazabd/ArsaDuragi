using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.PropertyImage;

namespace ArsaDuragi.Application.Interfaces.Services;

public interface IPropertyImageService
{
    Task<IReadOnlyList<PropertyImageDto>> AddImagesAsync(int propertyId, IReadOnlyList<UploadFile> files, CancellationToken ct = default);
    Task<PropertyImageDto> ReplaceImageAsync(int propertyId, int imageId, UploadFile file, CancellationToken ct = default);
    Task DeleteImageAsync(int propertyId, int imageId, CancellationToken ct = default);
    Task<IReadOnlyList<PropertyImageDto>> SetMainImageAsync(int propertyId, int imageId, CancellationToken ct = default);
}
