using System.Net;
using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.PropertyAd;
using ArsaDuragi.Application.Exceptions;
using ArsaDuragi.Application.Interfaces.Repositories;
using ArsaDuragi.Application.Interfaces.Services;
using ArsaDuragi.Application.Interfaces.Storage;
using ArsaDuragi.Domain.Entities;
using ArsaDuragi.Domain.Enums;
using Microsoft.Extensions.Options;

namespace ArsaDuragi.Application.Services;

public sealed class PropertyAdService : IPropertyAdService
{
    private readonly IPropertyAdRepository _properties;
    private readonly IDistrictRepository _districts;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _current;
    private readonly IFileStorageService _storage;
    private readonly IEmailService _email;
    private readonly PropertyDtoMapper _mapper;
    private readonly ModerationOptions _moderation;

    public PropertyAdService(
        IPropertyAdRepository properties,
        IDistrictRepository districts,
        IUnitOfWork uow,
        ICurrentUserService current,
        IFileStorageService storage,
        IEmailService email,
        PropertyDtoMapper mapper,
        IOptions<ModerationOptions> moderation)
    {
        _properties = properties;
        _districts = districts;
        _uow = uow;
        _current = current;
        _storage = storage;
        _email = email;
        _mapper = mapper;
        _moderation = moderation.Value;
    }

    // ------------------------------------------------------------------ public reads

    public async Task<PagedResult<PropertyAdListDto>> GetPublicAsync(PropertyAdFilterDto filter, CancellationToken ct = default)
    {
        // Trusted restriction decided by the server: the public only ever sees Active listings.
        var criteria = new PropertyAdSearchCriteria { Filter = filter, Statuses = new[] { PropertyStatus.Active } };
        var page = await _properties.SearchAsync(criteria, ct);
        return page.Map(_mapper.ToList);
    }

    public async Task<IReadOnlyList<PropertyAdListDto>> GetFeaturedAsync(int take, CancellationToken ct = default)
    {
        var items = await _properties.GetFeaturedAsync(Math.Clamp(take, 1, 24), ct);
        return items.Select(_mapper.ToList).ToList();
    }

    public async Task<PropertyAdGetDto> GetByIdOrSlugAsync(string idOrSlug, CancellationToken ct = default)
    {
        var property = int.TryParse(idOrSlug, out var id)
            ? await _properties.GetByIdAsync(id, false, ct)
            : await _properties.GetBySlugAsync(idOrSlug, false, ct);

        // Not-visible listings look exactly like missing ones, so their existence is not leaked.
        if (property is null || (property.Status != PropertyStatus.Active && !IsOwnerOrAdmin(property)))
            throw new NotFoundException("Property not found.");

        var dto = _mapper.ToDetails(property);
        if (!IsOwnerOrAdmin(property)) dto.RejectionReason = null;
        return dto;
    }

    // ------------------------------------------------------------------ my listings / admin list

    public async Task<PagedResult<PropertyAdListDto>> GetMineAsync(PropertyAdFilterDto filter, PropertyStatus? status, CancellationToken ct = default)
    {
        var userId = OwnershipGuard.RequireUserId(_current);
        var criteria = new PropertyAdSearchCriteria
        {
            Filter = filter,
            OwnerId = userId,
            Statuses = status.HasValue ? new[] { status.Value } : null
        };
        var page = await _properties.SearchAsync(criteria, ct);
        return page.Map(_mapper.ToList);
    }

    public async Task<PagedResult<PropertyAdListDto>> GetAllForAdminAsync(PropertyAdFilterDto filter, PropertyStatus? status, CancellationToken ct = default)
    {
        OwnershipGuard.EnsureAdmin(_current);
        var criteria = new PropertyAdSearchCriteria
        {
            Filter = filter,
            Statuses = status.HasValue ? new[] { status.Value } : null
        };
        var page = await _properties.SearchAsync(criteria, ct);
        return page.Map(_mapper.ToList);
    }

    // ------------------------------------------------------------------ create / update / delete

    public async Task<PropertyAdGetDto> CreateAsync(PropertyAdCreateDto dto, CancellationToken ct = default)
    {
        var userId = OwnershipGuard.RequireUserId(_current); // from the JWT, never from the request body
        EnsurePostingAllowed();
        await EnsureLocationIsValidAsync(dto, ct);

        var property = new PropertyAd
        {
            Slug = await GenerateUniqueSlugAsync(dto.Title, ct),
            UserId = userId
        };
        Apply(property, dto);

        if (_current.IsAdmin || !_moderation.RequireApproval)
        {
            property.Status = PropertyStatus.Active;
            property.ListedAt = DateTime.UtcNow;
        }
        else
        {
            property.Status = PropertyStatus.Pending;
        }

        await _properties.AddAsync(property, ct);
        await _uow.SaveChangesAsync(ct);

        var saved = await _properties.GetByIdAsync(property.Id, false, ct)
                    ?? throw new NotFoundException("Property not found.");
        return _mapper.ToDetails(saved);
    }

    public async Task<PropertyAdGetDto> UpdateAsync(int id, PropertyAdUpdateDto dto, CancellationToken ct = default)
    {
        var property = await LoadForWriteAsync(id, ct);
        await EnsureLocationIsValidAsync(dto, ct);

        Apply(property, dto);

        // Edited content must be reviewed again (otherwise an approved listing could be turned into anything).
        if (!_current.IsAdmin && _moderation.RequireApproval &&
            property.Status is (PropertyStatus.Active or PropertyStatus.Rejected))
        {
            property.Status = PropertyStatus.Pending;
            property.RejectionReason = null;
            property.IsFeatured = false;
        }

        await _uow.SaveChangesAsync(ct);

        var saved = await _properties.GetByIdAsync(id, false, ct) ?? throw new NotFoundException("Property not found.");
        return _mapper.ToDetails(saved);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var property = await LoadForWriteAsync(id, ct);

        var keys = property.Images.Where(i => i.ObjectKey != null).Select(i => i.ObjectKey!).ToList();

        // Soft delete: the row stays for auditing, but it disappears from every query (global query filter).
        property.IsDeleted = true;
        property.DeletedAt = DateTime.UtcNow;
        property.Status = PropertyStatus.Archived;
        property.IsFeatured = false;
        property.Images.Clear(); // images are user content: remove rows and files

        await _uow.SaveChangesAsync(ct);

        foreach (var key in keys) await _storage.DeleteAsync(key, ct);
    }

    public async Task<PropertyAdGetDto> ChangeStatusAsync(int id, PropertyAdStatusChangeDto dto, CancellationToken ct = default)
    {
        var property = await LoadForWriteAsync(id, ct);
        var target = dto.Status;

        switch (dto.Status)
        {
            case PropertyStatus.Sold:
                if (property.Status != PropertyStatus.Active)
                    throw new BadRequestException("Only active listings can be marked as sold.");
                break;

            case PropertyStatus.Archived:
                if (property.Status == PropertyStatus.Archived)
                    throw new BadRequestException("The listing is already archived.");
                break;

            case PropertyStatus.Pending: // "resubmit"
                if (property.Status is not (PropertyStatus.Archived or PropertyStatus.Rejected))
                    throw new BadRequestException("Only archived or rejected listings can be resubmitted.");
                target = _current.IsAdmin || !_moderation.RequireApproval ? PropertyStatus.Active : PropertyStatus.Pending;
                break;

            default:
                throw new BadRequestException("Status can only be changed to Sold, Archived or Pending (resubmit).");
        }

        property.Status = target;
        property.RejectionReason = null;
        if (target == PropertyStatus.Active) property.ListedAt ??= DateTime.UtcNow;
        if (target != PropertyStatus.Active) property.IsFeatured = false;

        await _uow.SaveChangesAsync(ct);
        return _mapper.ToDetails(property);
    }

    // ------------------------------------------------------------------ moderation (Admin)

    public async Task<PropertyAdGetDto> ApproveAsync(int id, CancellationToken ct = default)
    {
        OwnershipGuard.EnsureAdmin(_current);
        var property = await _properties.GetByIdAsync(id, true, ct) ?? throw new NotFoundException("Property not found.");

        if (property.Status is not (PropertyStatus.Pending or PropertyStatus.Rejected))
            throw new BadRequestException("Only pending or rejected listings can be approved.");

        property.Status = PropertyStatus.Active;
        property.ListedAt ??= DateTime.UtcNow;
        property.RejectionReason = null;
        property.ReviewedAt = DateTime.UtcNow;
        property.ReviewedById = _current.UserId;

        await _uow.SaveChangesAsync(ct);

        await NotifyOwnerAsync(property, "Your listing was approved",
            $"<p>Your listing <strong>{WebUtility.HtmlEncode(property.Title)}</strong> was approved and is now visible on ArsaDuragi.</p>", ct);

        return _mapper.ToDetails(property);
    }

    public async Task<PropertyAdGetDto> RejectAsync(int id, RejectPropertyAdDto dto, CancellationToken ct = default)
    {
        OwnershipGuard.EnsureAdmin(_current);
        var property = await _properties.GetByIdAsync(id, true, ct) ?? throw new NotFoundException("Property not found.");

        if (property.Status != PropertyStatus.Pending && property.Status != PropertyStatus.Active)
            throw new BadRequestException("Only pending or active listings can be rejected.");

        property.Status = PropertyStatus.Rejected;
        property.RejectionReason = dto.Reason.Trim();
        property.IsFeatured = false;
        property.ReviewedAt = DateTime.UtcNow;
        property.ReviewedById = _current.UserId;

        await _uow.SaveChangesAsync(ct);

        await NotifyOwnerAsync(property, "Your listing was rejected",
            $"<p>Your listing <strong>{WebUtility.HtmlEncode(property.Title)}</strong> was rejected.</p>" +
            $"<p>Reason: {WebUtility.HtmlEncode(property.RejectionReason)}</p>", ct);

        return _mapper.ToDetails(property);
    }

    public async Task<PropertyAdGetDto> SetFeaturedAsync(int id, bool isFeatured, CancellationToken ct = default)
    {
        OwnershipGuard.EnsureAdmin(_current);
        var property = await _properties.GetByIdAsync(id, true, ct) ?? throw new NotFoundException("Property not found.");

        if (isFeatured && property.Status != PropertyStatus.Active)
            throw new BadRequestException("Only active listings can be featured.");

        property.IsFeatured = isFeatured;
        await _uow.SaveChangesAsync(ct);
        return _mapper.ToDetails(property);
    }

    // ------------------------------------------------------------------ helpers

    private bool IsOwnerOrAdmin(PropertyAd property)
        => _current.IsAdmin || (_current.UserId is not null && _current.UserId == property.UserId);

    private void EnsurePostingAllowed()
    {
        if (_moderation.AdminOnlyPosting && !_current.IsAdmin)
            throw new ForbiddenException("Only administrators can publish and manage listings.");
    }

    private async Task<PropertyAd> LoadForWriteAsync(int id, CancellationToken ct)
    {
        EnsurePostingAllowed();
        var property = await _properties.GetByIdAsync(id, true, ct) ?? throw new NotFoundException("Property not found.");
        OwnershipGuard.EnsureOwnerOrAdmin(_current, property.UserId);
        return property;
    }

    private async Task EnsureLocationIsValidAsync(PropertyAdWriteDto dto, CancellationToken ct)
    {
        var district = await _districts.GetByIdAsync(dto.DistrictId, false, ct)
                       ?? throw new BadRequestException("The selected district does not exist.");

        if (district.CityId != dto.CityId)
            throw new BadRequestException("The selected district does not belong to the selected city.");
    }

    private static void Apply(PropertyAd property, PropertyAdWriteDto dto)
    {
        property.Title = dto.Title.Trim();
        property.Description = dto.Description.Trim();
        property.Address = dto.Address.Trim();
        property.Price = dto.Price;
        property.Area = dto.Area;
        property.PropertyType = dto.PropertyType;
        property.Zoning = dto.Zoning;
        property.Terrain = dto.Terrain;
        property.RoadAccess = dto.RoadAccess;
        property.RoadDistanceMeters = PropertyLabels.RequiresDistance(dto.RoadAccess) ? dto.RoadDistanceMeters : null;
        property.Utilities = PropertyLabels.ToFlags(dto.Utilities);
        property.CityId = dto.CityId;
        property.DistrictId = dto.DistrictId;
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, CancellationToken ct)
    {
        var baseSlug = SlugHelper.Generate(title);
        for (var attempt = 0; attempt < 5; attempt++)
        {
            // A random suffix also guarantees the slug is never a plain number (which would look like an id).
            var candidate = $"{baseSlug}-{Guid.NewGuid().ToString("N")[..6]}";
            if (!await _properties.SlugExistsAsync(candidate, ct)) return candidate;
        }
        return $"{baseSlug}-{Guid.NewGuid():N}";
    }

    private async Task NotifyOwnerAsync(PropertyAd property, string subject, string html, CancellationToken ct)
    {
        var to = property.User?.Email;
        if (string.IsNullOrWhiteSpace(to) || to.EndsWith(".invalid", StringComparison.OrdinalIgnoreCase)) return;
        await _email.SendAsync(to, subject, html, ct); // EmailService never throws: it logs failures
    }
}
