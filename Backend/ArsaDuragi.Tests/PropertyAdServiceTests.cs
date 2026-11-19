using ArsaDuragi.Application.DTOs.PropertyAd;
using ArsaDuragi.Application.Exceptions;
using ArsaDuragi.Domain.Entities;
using ArsaDuragi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ArsaDuragi.Tests;

public class PropertyAdServiceTests
{
    private static PropertyAdCreateDto NewDto(TestEnv env) => new()
    {
        Title = "Merdakan'da konut imarlı arsa",
        Description = "A long enough description for the validator and the tests.",
        Address = "Merdakan, Bakü",
        Price = 180_000,
        Area = 600,
        PropertyType = PropertyType.Land,
        Zoning = PropertyZoning.Residential,
        Terrain = TerrainType.Flat,
        RoadAccess = RoadAccessType.AsphaltFrontage,
        Utilities = new List<UtilityType> { UtilityType.Electricity, UtilityType.Water },
        CityId = env.City.Id,
        DistrictId = env.Merdakan.Id
    };

    private static PropertyAdUpdateDto NewUpdate(TestEnv env, string title) => new()
    {
        Title = title,
        Description = "An updated description that is long enough.",
        Address = "Merdakan, Bakü",
        Price = 190_000,
        Area = 600,
        PropertyType = PropertyType.Land,
        Zoning = PropertyZoning.Residential,
        Terrain = TerrainType.Flat,
        RoadAccess = RoadAccessType.MainRoadNearby,
        RoadDistanceMeters = 300,
        Utilities = new List<UtilityType> { UtilityType.Water },
        CityId = env.City.Id,
        DistrictId = env.Merdakan.Id
    };

    [Fact]
    public async Task Create_sets_pending_and_takes_owner_from_current_user()
    {
        using var env = new TestEnv();
        env.LoginAs(env.Owner);

        var created = await env.PropertyService().CreateAsync(NewDto(env));

        Assert.Equal("Pending", created.Status);
        Assert.False(created.Featured);
        Assert.Equal("Owner One", created.OwnerName);
        Assert.Equal(env.Owner.Id, (await env.Db.PropertyAds.SingleAsync()).UserId);
        Assert.Equal(new[] { "Elektrik", "Su" }, created.Utilities);
        Assert.Equal("Konut imarlı", created.Zoning);
    }

    [Fact]
    public async Task Create_by_admin_is_published_immediately()
    {
        using var env = new TestEnv();
        env.LoginAs(env.Admin, admin: true);

        var created = await env.PropertyService().CreateAsync(NewDto(env));

        Assert.Equal("Active", created.Status);
        Assert.NotNull(created.ListedAt);
    }

    [Fact]
    public async Task Create_without_moderation_is_published_immediately()
    {
        using var env = new TestEnv(requireApproval: false);
        env.LoginAs(env.Owner);

        var created = await env.PropertyService().CreateAsync(NewDto(env));

        Assert.Equal("Active", created.Status);
    }

    [Fact]
    public async Task Create_rejects_district_from_another_city()
    {
        using var env = new TestEnv();
        env.LoginAs(env.Owner);
        var dto = NewDto(env);
        dto.DistrictId = env.OtherCityDistrict.Id; // belongs to a different city than dto.CityId

        await Assert.ThrowsAsync<BadRequestException>(() => env.PropertyService().CreateAsync(dto));
    }

    [Fact]
    public async Task Create_requires_authentication()
    {
        using var env = new TestEnv();
        await Assert.ThrowsAsync<UnauthorizedException>(() => env.PropertyService().CreateAsync(NewDto(env)));
    }

    [Fact]
    public async Task Owner_can_update_own_property_and_it_goes_back_to_moderation()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500, PropertyStatus.Active);
        env.LoginAs(env.Owner);

        var updated = await env.PropertyService().UpdateAsync(ad.Id, NewUpdate(env, "New title here"));

        Assert.Equal("New title here", updated.Title);
        Assert.Equal("Ana yola 300 m", updated.RoadAccess);
        Assert.Equal("Pending", updated.Status);
    }

    [Fact]
    public async Task Cannot_update_another_users_property()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500);
        env.LoginAs(env.Stranger);

        await Assert.ThrowsAsync<ForbiddenException>(() => env.PropertyService().UpdateAsync(ad.Id, NewUpdate(env, "Hacked title")));

        Assert.NotEqual("Hacked title", (await env.Db.PropertyAds.SingleAsync()).Title);
    }

    [Fact]
    public async Task Cannot_delete_another_users_property()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500);
        env.LoginAs(env.Stranger);

        await Assert.ThrowsAsync<ForbiddenException>(() => env.PropertyService().DeleteAsync(ad.Id));
    }

    [Fact]
    public async Task Admin_can_update_any_property_without_changing_status()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500, PropertyStatus.Active);
        env.LoginAs(env.Admin, admin: true);

        var updated = await env.PropertyService().UpdateAsync(ad.Id, NewUpdate(env, "Admin edited title"));

        Assert.Equal("Active", updated.Status);
    }

    [Fact]
    public async Task Owner_delete_is_soft_and_removes_images()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500);
        ad.Images.Add(new PropertyImage { ImageUrl = "/uploads/properties/1/a.jpg", ObjectKey = "properties/1/a.jpg", IsMain = true });
        await env.Db.SaveChangesAsync();
        env.LoginAs(env.Owner);

        await env.PropertyService().DeleteAsync(ad.Id);

        Assert.Empty(await env.Db.PropertyAds.ToListAsync());                                    // hidden by the query filter
        Assert.Single(await env.Db.PropertyAds.IgnoreQueryFilters().ToListAsync());              // row still exists
        Assert.Contains("properties/1/a.jpg", env.Storage.Deleted);                              // file removed
        await Assert.ThrowsAsync<NotFoundException>(() => env.PropertyService().GetByIdOrSlugAsync(ad.Id.ToString()));
    }

    [Fact]
    public async Task Pending_listing_is_hidden_from_strangers_but_visible_to_owner_and_admin()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500, PropertyStatus.Pending);

        env.LoginAs(env.Stranger);
        await Assert.ThrowsAsync<NotFoundException>(() => env.PropertyService().GetByIdOrSlugAsync(ad.Slug));

        env.LoginAs(env.Owner);
        Assert.Equal(ad.Id, (await env.PropertyService().GetByIdOrSlugAsync(ad.Slug)).Id);

        env.LoginAs(env.Admin, admin: true);
        Assert.Equal(ad.Id, (await env.PropertyService().GetByIdOrSlugAsync(ad.Id.ToString())).Id);
    }

    [Fact]
    public async Task Admin_approves_pending_listing_and_it_becomes_public()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500, PropertyStatus.Pending);
        ad.ListedAt = null;
        await env.Db.SaveChangesAsync();

        env.LoginAs(env.Admin, admin: true);
        var approved = await env.PropertyService().ApproveAsync(ad.Id);

        Assert.Equal("Active", approved.Status);
        Assert.NotNull(approved.ListedAt);

        env.Current.UserId = null;
        env.Current.IsAdmin = false;
        var page = await env.PropertyService().GetPublicAsync(new PropertyAdFilterDto());
        Assert.Equal(1, page.TotalCount);
    }

    [Fact]
    public async Task Admin_reject_stores_reason_and_notifies_owner()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500, PropertyStatus.Pending);
        env.LoginAs(env.Admin, admin: true);

        var rejected = await env.PropertyService().RejectAsync(ad.Id, new RejectPropertyAdDto { Reason = "Photos are missing" });

        Assert.Equal("Rejected", rejected.Status);
        Assert.Equal("Photos are missing", rejected.RejectionReason);
        Assert.Contains(env.Email.Sent, s => s.StartsWith("owner@test.az"));

        // a stranger must not see the rejection reason, and cannot see the listing at all
        env.LoginAs(env.Stranger);
        await Assert.ThrowsAsync<NotFoundException>(() => env.PropertyService().GetByIdOrSlugAsync(ad.Slug));
    }

    [Fact]
    public async Task Normal_user_cannot_approve_reject_or_feature()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500, PropertyStatus.Pending);
        env.LoginAs(env.Owner);

        await Assert.ThrowsAsync<ForbiddenException>(() => env.PropertyService().ApproveAsync(ad.Id));
        await Assert.ThrowsAsync<ForbiddenException>(() => env.PropertyService().RejectAsync(ad.Id, new RejectPropertyAdDto { Reason = "x" }));
        await Assert.ThrowsAsync<ForbiddenException>(() => env.PropertyService().SetFeaturedAsync(ad.Id, true));
    }

    [Fact]
    public async Task Only_active_listings_can_be_featured_by_admin()
    {
        using var env = new TestEnv();
        var pending = env.AddProperty(env.Merdakan, 100_000, 500, PropertyStatus.Pending);
        var active = env.AddProperty(env.Merdakan, 100_000, 500, PropertyStatus.Active);
        env.LoginAs(env.Admin, admin: true);

        await Assert.ThrowsAsync<BadRequestException>(() => env.PropertyService().SetFeaturedAsync(pending.Id, true));
        var featured = await env.PropertyService().SetFeaturedAsync(active.Id, true);

        Assert.True(featured.Featured);
        Assert.Single(await env.PropertyService().GetFeaturedAsync(6));
    }

    [Fact]
    public async Task My_listings_include_pending_and_only_mine()
    {
        using var env = new TestEnv();
        env.AddProperty(env.Merdakan, 1, 1, PropertyStatus.Pending, owner: env.Owner);
        env.AddProperty(env.Merdakan, 1, 1, PropertyStatus.Active, owner: env.Owner);
        env.AddProperty(env.Merdakan, 1, 1, PropertyStatus.Active, owner: env.Stranger);
        env.LoginAs(env.Owner);

        var mine = await env.PropertyService().GetMineAsync(new PropertyAdFilterDto(), null);
        var minePending = await env.PropertyService().GetMineAsync(new PropertyAdFilterDto(), PropertyStatus.Pending);

        Assert.Equal(2, mine.TotalCount);
        Assert.Equal(1, minePending.TotalCount);
    }

    [Fact]
    public async Task Owner_can_mark_active_listing_as_sold_but_not_pending()
    {
        using var env = new TestEnv();
        var active = env.AddProperty(env.Merdakan, 1, 1, PropertyStatus.Active);
        var pending = env.AddProperty(env.Merdakan, 1, 1, PropertyStatus.Pending);
        env.LoginAs(env.Owner);

        var sold = await env.PropertyService().ChangeStatusAsync(active.Id, new PropertyAdStatusChangeDto { Status = PropertyStatus.Sold });
        Assert.Equal("Sold", sold.Status);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            env.PropertyService().ChangeStatusAsync(pending.Id, new PropertyAdStatusChangeDto { Status = PropertyStatus.Sold }));
        await Assert.ThrowsAsync<BadRequestException>(() =>
            env.PropertyService().ChangeStatusAsync(active.Id, new PropertyAdStatusChangeDto { Status = PropertyStatus.Active })); // owner cannot self-approve
    }

    // ----- "only the admin posts listings" mode (the default of the real application) -----

    [Fact]
    public async Task Admin_only_mode_customers_cannot_create_edit_delete_or_manage_images()
    {
        using var env = new TestEnv(adminOnlyPosting: true);
        var ad = env.AddProperty(env.Merdakan, 100_000, 500, PropertyStatus.Active, owner: env.Owner);
        env.LoginAs(env.Owner); // even the "owner" of the row is a customer here

        await Assert.ThrowsAsync<ForbiddenException>(() => env.PropertyService().CreateAsync(NewDto(env)));
        await Assert.ThrowsAsync<ForbiddenException>(() => env.PropertyService().UpdateAsync(ad.Id, NewUpdate(env, "Customer edit")));
        await Assert.ThrowsAsync<ForbiddenException>(() => env.PropertyService().DeleteAsync(ad.Id));
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            env.PropertyService().ChangeStatusAsync(ad.Id, new PropertyAdStatusChangeDto { Status = PropertyStatus.Sold }));
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            env.ImageService().AddImagesAsync(ad.Id, new[] { new ArsaDuragi.Application.Common.UploadFile(new MemoryStream(new byte[] { 1 }), "a.jpg", 1) }));
    }

    [Fact]
    public async Task Admin_only_mode_admin_publishes_directly_and_customers_can_still_browse()
    {
        using var env = new TestEnv(adminOnlyPosting: true);
        env.LoginAs(env.Admin, admin: true);

        var created = await env.PropertyService().CreateAsync(NewDto(env));
        Assert.Equal("Active", created.Status);

        env.LoginAs(env.Stranger); // a normal customer
        var page = await env.PropertyService().GetPublicAsync(new PropertyAdFilterDto());
        Assert.Equal(1, page.TotalCount);
        Assert.Equal(created.Id, (await env.PropertyService().GetByIdOrSlugAsync(created.Slug)).Id);
    }
}
