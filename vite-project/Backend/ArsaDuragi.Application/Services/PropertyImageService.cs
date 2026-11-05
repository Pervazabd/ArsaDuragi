using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.PropertyImage;
using ArsaDuragi.Application.Exceptions;
using ArsaDuragi.Application.Interfaces.Repositories;
using ArsaDuragi.Application.Interfaces.Services;
using ArsaDuragi.Application.Interfaces.Storage;
using ArsaDuragi.Domain.Entities;
using Microsoft.Extensions.Options;

namespace ArsaDuragi.Application.Services;

public sealed class PropertyImageService : IPropertyImageService
{
    private readonly IPropertyAdRepository _properties;
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _storage;
    private readonly ICurrentUserService _current;
    private readonly PropertyDtoMapper _mapper;
    private readonly ModerationOptions _moderation;

    public PropertyImageService(IPropertyAdRepository properties, IUnitOfWork uow, IFileStorageService storage,
        ICurrentUserService current, PropertyDtoMapper mapper, IOptions<ModerationOptions> moderation)
    {
        _moderation = moderation.Value;
        _properties = properties;
        _uow = uow;
        _storage = storage;
        _current = current;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PropertyImageDto>> AddImagesAsync(int propertyId, IReadOnlyList<UploadFile> files, CancellationToken ct = default)
    {
        var property = await LoadForWriteAsync(propertyId, ct);

        if (files.Count == 0)
            throw new BadRequestException("No files were uploaded.");
        if (property.Images.Count + files.Count > UploadLimits.MaxImagesPerProperty)
            throw new BadRequestException($"A listing can have at most {UploadLimits.MaxImagesPerProperty} images.");

        var saved = new List<StoredFileResult>();
        var created = new List<PropertyImage>();
        try
        {
            foreach (var file in files)
            {
                var stored = await _storage.SaveImageAsync(file.Content, file.FileName, $"properties/{property.Id}", ct);
                saved.Add(stored);

                var image = new PropertyImage { ImageUrl = stored.Url, ObjectKey = stored.ObjectKey };
                property.Images.Add(image);
                created.Add(image);
            }

            if (!property.Images.Any(i => i.IsMain))
                created[0].IsMain = true;

            await _uow.SaveChangesAsync(ct);
        }
        catch
        {
            // Do not leave orphan files on disk if validation or the database failed half-way.
            foreach (var s in saved) await _storage.DeleteAsync(s.ObjectKey, CancellationToken.None);
            throw;
        }

        return created.Select(_mapper.ToImage).ToList();
    }

    public async Task<PropertyImageDto> ReplaceImageAsync(int propertyId, int imageId, UploadFile file, CancellationToken ct = default)
    {
        var property = await LoadForWriteAsync(propertyId, ct);
        var image = FindImage(property, imageId);

        var stored = await _storage.SaveImageAsync(file.Content, file.FileName, $"properties/{property.Id}", ct);
        var oldKey = image.ObjectKey;

        image.ImageUrl = stored.Url;
        image.ObjectKey = stored.ObjectKey;

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch
        {
            await _storage.DeleteAsync(stored.ObjectKey, CancellationToken.None);
            throw;
        }

        await _storage.DeleteAsync(oldKey, ct);
        return _mapper.ToImage(image);
    }

    public async Task DeleteImageAsync(int propertyId, int imageId, CancellationToken ct = default)
    {
        var property = await LoadForWriteAsync(propertyId, ct);
        var image = FindImage(property, imageId);
        var key = image.ObjectKey;

        property.Images.Remove(image); // required relationship: removing it from the collection deletes the row

        if (image.IsMain)
        {
            var next = property.Images.OrderBy(i => i.Id).FirstOrDefault();
            if (next is not null) next.IsMain = true;
        }

        await _uow.SaveChangesAsync(ct);
        await _storage.DeleteAsync(key, ct);
    }

    public async Task<IReadOnlyList<PropertyImageDto>> SetMainImageAsync(int propertyId, int imageId, CancellationToken ct = default)
    {
        var property = await LoadForWriteAsync(propertyId, ct);
        var target = FindImage(property, imageId);

        // "Exactly one main image" is enforced here in the service (see README for why not in the database).
        foreach (var image in property.Images) image.IsMain = image.Id == target.Id;

        await _uow.SaveChangesAsync(ct);
        return property.Images.OrderByDescending(i => i.IsMain).ThenBy(i => i.Id).Select(_mapper.ToImage).ToList();
    }

    private async Task<PropertyAd> LoadForWriteAsync(int propertyId, CancellationToken ct)
    {
        if (_moderation.AdminOnlyPosting && !_current.IsAdmin)
            throw new ForbiddenException("Only administrators can publish and manage listings.");

        var property = await _properties.GetByIdAsync(propertyId, true, ct) ?? throw new NotFoundException("Property not found.");
        OwnershipGuard.EnsureOwnerOrAdmin(_current, property.UserId);
        return property;
    }

    private static PropertyImage FindImage(PropertyAd property, int imageId)
        => property.Images.FirstOrDefault(i => i.Id == imageId) ?? throw new NotFoundException("Image not found.");
}
