using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.Interfaces.Services;
using ArsaDuragi.Application.Interfaces.Storage;
using ArsaDuragi.Application.Mappings;
using ArsaDuragi.Application.Services;
using ArsaDuragi.Domain.Entities;
using ArsaDuragi.Domain.Enums;
using ArsaDuragi.Persistence.Context;
using ArsaDuragi.Persistence.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ArsaDuragi.Tests;

public sealed class FakeCurrentUser : ICurrentUserService
{
    public string? UserId { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsAuthenticated => UserId is not null;
}

public sealed class FakeStorage : IFileStorageService
{
    public List<string> Deleted { get; } = new();

    public Task<StoredFileResult> SaveImageAsync(Stream content, string originalFileName, string folder, CancellationToken ct = default)
    {
        var key = $"{folder}/{Guid.NewGuid():N}.jpg";
        return Task.FromResult(new StoredFileResult(key, "/uploads/" + key));
    }

    public Task DeleteAsync(string? objectKey, CancellationToken ct = default)
    {
        if (objectKey is not null) Deleted.Add(objectKey);
        return Task.CompletedTask;
    }

    public string? GetPublicUrl(string? urlOrPath) => urlOrPath;
}

public sealed class FakeEmail : IEmailService
{
    public List<string> Sent { get; } = new();

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        Sent.Add($"{to}|{subject}");
        return Task.CompletedTask;
    }
}

/// <summary>A tiny "composition root" for tests: real repositories + real services on an in-memory database.</summary>
public sealed class TestEnv : IDisposable
{
    public AppDbContext Db { get; }
    public FakeCurrentUser Current { get; } = new();
    public FakeStorage Storage { get; } = new();
    public FakeEmail Email { get; } = new();
    public IMapper Mapper { get; }
    public PropertyDtoMapper DtoMapper { get; }

    public User Owner { get; private set; } = null!;
    public User Stranger { get; private set; } = null!;
    public User Admin { get; private set; } = null!;
    public City City { get; private set; } = null!;
    public District Merdakan { get; private set; } = null!;
    public District Bilgah { get; private set; } = null!;
    public City OtherCity { get; private set; } = null!;
    public District OtherCityDistrict { get; private set; } = null!;

    public TestEnv(bool requireApproval = true, bool adminOnlyPosting = false)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("arsa-" + Guid.NewGuid())
            .Options;
        Db = new AppDbContext(options);

        Mapper = new MapperConfiguration(c => c.AddProfile<MappingProfile>()).CreateMapper();
        DtoMapper = new PropertyDtoMapper(Mapper, Storage);
        RequireApproval = requireApproval;
        AdminOnlyPosting = adminOnlyPosting;
        Seed();
    }

    public bool RequireApproval { get; }
    public bool AdminOnlyPosting { get; }

    private void Seed()
    {
        Owner = new User { Id = "owner-1", UserName = "owner@test.az", Email = "owner@test.az", FullName = "Owner One", PhoneNumber = "+994501112233" };
        Stranger = new User { Id = "stranger-1", UserName = "x@test.az", Email = "x@test.az", FullName = "Stranger" };
        Admin = new User { Id = "admin-1", UserName = "admin@test.az", Email = "admin@test.az", FullName = "Admin" };
        Db.Users.AddRange(Owner, Stranger, Admin);

        City = new City { Name = "Bakü" };
        Merdakan = new District { Name = "Merdakan", City = City };
        Bilgah = new District { Name = "Bilgah", City = City };
        OtherCity = new City { Name = "Sumqayıt" };
        OtherCityDistrict = new District { Name = "Centre", City = OtherCity };
        Db.Cities.AddRange(City, OtherCity);
        Db.Districts.AddRange(Merdakan, Bilgah, OtherCityDistrict);
        Db.SaveChanges();
    }

    public PropertyAdService PropertyService() => new(
        new PropertyAdRepository(Db), new DistrictRepository(Db), Db, Current, Storage, Email, DtoMapper,
        Options.Create(new ModerationOptions { RequireApproval = RequireApproval, AdminOnlyPosting = AdminOnlyPosting }));

    public PropertyImageService ImageService() => new(new PropertyAdRepository(Db), Db, Storage, Current, DtoMapper,
        Options.Create(new ModerationOptions { RequireApproval = RequireApproval, AdminOnlyPosting = AdminOnlyPosting }));

    public FavoriteService FavoriteService() => new(new FavoriteRepository(Db), new PropertyAdRepository(Db), Db, Current, DtoMapper);

    public PropertyAdRepository PropertyRepo() => new(Db);

    public void LoginAs(User user, bool admin = false)
    {
        Current.UserId = user.Id;
        Current.IsAdmin = admin;
    }

    public PropertyAd AddProperty(District district, decimal price, decimal area, PropertyStatus status = PropertyStatus.Active,
        string? title = null, bool featured = false, DateTime? listedAt = null, User? owner = null)
    {
        var p = new PropertyAd
        {
            Slug = "slug-" + Guid.NewGuid().ToString("N")[..8],
            Title = title ?? $"Listing {price}/{area}",
            Description = "A description that is long enough to be valid.",
            Address = district.Name + ", Bakü",
            Price = price,
            Area = area,
            PropertyType = PropertyType.Land,
            Zoning = PropertyZoning.Residential,
            Terrain = TerrainType.Flat,
            RoadAccess = RoadAccessType.AsphaltFrontage,
            Utilities = UtilityType.Electricity | UtilityType.Water,
            Status = status,
            IsFeatured = featured,
            ListedAt = listedAt ?? DateTime.UtcNow,
            UserId = (owner ?? Owner).Id,
            CityId = district.CityId,
            DistrictId = district.Id
        };
        Db.PropertyAds.Add(p);
        Db.SaveChanges();
        return p;
    }

    public void Dispose() => Db.Dispose();
}
