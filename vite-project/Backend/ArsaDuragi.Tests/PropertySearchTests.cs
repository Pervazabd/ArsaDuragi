using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.PropertyAd;
using ArsaDuragi.Domain.Enums;
using Xunit;

namespace ArsaDuragi.Tests;

public class PropertySearchTests
{
    private static PropertyAdSearchCriteria Public(PropertyAdFilterDto f)
        => new() { Filter = f, Statuses = new[] { PropertyStatus.Active } };

    [Fact]
    public async Task Public_search_returns_only_active_listings()
    {
        using var env = new TestEnv();
        env.AddProperty(env.Merdakan, 100_000, 500);
        env.AddProperty(env.Merdakan, 110_000, 500, PropertyStatus.Pending);
        env.AddProperty(env.Merdakan, 120_000, 500, PropertyStatus.Rejected);

        var result = await env.PropertyRepo().SearchAsync(Public(new PropertyAdFilterDto()));

        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Filter_by_district()
    {
        using var env = new TestEnv();
        env.AddProperty(env.Merdakan, 100_000, 500);
        env.AddProperty(env.Bilgah, 100_000, 500);
        env.AddProperty(env.Bilgah, 120_000, 700);

        var result = await env.PropertyRepo().SearchAsync(Public(new PropertyAdFilterDto { DistrictId = env.Bilgah.Id }));

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, p => Assert.Equal(env.Bilgah.Id, p.DistrictId));
    }

    [Fact]
    public async Task Filter_by_price_range_is_inclusive()
    {
        using var env = new TestEnv();
        env.AddProperty(env.Merdakan, 50_000, 500);
        env.AddProperty(env.Merdakan, 100_000, 500);
        env.AddProperty(env.Merdakan, 200_000, 500);
        env.AddProperty(env.Merdakan, 300_000, 500);

        var result = await env.PropertyRepo().SearchAsync(Public(new PropertyAdFilterDto { MinPrice = 100_000, MaxPrice = 200_000 }));

        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task Filters_work_together()
    {
        using var env = new TestEnv();
        env.AddProperty(env.Merdakan, 100_000, 600);
        env.AddProperty(env.Merdakan, 100_000, 100);
        env.AddProperty(env.Bilgah, 100_000, 600);

        var result = await env.PropertyRepo().SearchAsync(Public(new PropertyAdFilterDto
        {
            DistrictId = env.Merdakan.Id, MinArea = 500, MaxArea = 2000, MaxPrice = 150_000
        }));

        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Sort_by_area_descending_and_ascending()
    {
        using var env = new TestEnv();
        env.AddProperty(env.Merdakan, 100_000, 400);
        env.AddProperty(env.Merdakan, 100_000, 1200);
        env.AddProperty(env.Merdakan, 100_000, 800);

        var desc = await env.PropertyRepo().SearchAsync(Public(new PropertyAdFilterDto { SortBy = "area", SortOrder = "desc" }));
        var asc = await env.PropertyRepo().SearchAsync(Public(new PropertyAdFilterDto { SortBy = "area", SortOrder = "asc" }));

        Assert.Equal(new[] { 1200m, 800m, 400m }, desc.Items.Select(p => p.Area));
        Assert.Equal(new[] { 400m, 800m, 1200m }, asc.Items.Select(p => p.Area));
    }

    [Fact]
    public async Task Sort_by_price_ascending()
    {
        using var env = new TestEnv();
        env.AddProperty(env.Merdakan, 300_000, 500);
        env.AddProperty(env.Merdakan, 100_000, 500);
        env.AddProperty(env.Merdakan, 200_000, 500);

        var result = await env.PropertyRepo().SearchAsync(Public(new PropertyAdFilterDto { SortBy = "price", SortOrder = "asc" }));

        Assert.Equal(new[] { 100_000m, 200_000m, 300_000m }, result.Items.Select(p => p.Price));
    }

    [Fact]
    public async Task Default_sort_is_newest_first()
    {
        using var env = new TestEnv();
        env.AddProperty(env.Merdakan, 1, 1, title: "old", listedAt: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        env.AddProperty(env.Merdakan, 1, 1, title: "new", listedAt: new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc));

        var result = await env.PropertyRepo().SearchAsync(Public(new PropertyAdFilterDto()));

        Assert.Equal("new", result.Items[0].Title);
    }

    [Fact]
    public async Task Pagination_returns_correct_page_and_totals()
    {
        using var env = new TestEnv();
        for (var i = 1; i <= 25; i++) env.AddProperty(env.Merdakan, i * 1000, 500);

        var page3 = await env.PropertyRepo().SearchAsync(Public(new PropertyAdFilterDto { Page = 3, PageSize = 10, SortBy = "price", SortOrder = "asc" }));

        Assert.Equal(25, page3.TotalCount);
        Assert.Equal(3, page3.TotalPages);
        Assert.Equal(5, page3.Items.Count);
        Assert.Equal(21_000m, page3.Items[0].Price);
    }

    [Fact]
    public async Task Search_matches_title_and_district_name()
    {
        using var env = new TestEnv();
        env.AddProperty(env.Merdakan, 100_000, 500, title: "Sea view plot");
        env.AddProperty(env.Bilgah, 100_000, 500, title: "Quiet plot");

        var byTitle = await env.PropertyRepo().SearchAsync(Public(new PropertyAdFilterDto { Search = "Sea view" }));
        var byDistrict = await env.PropertyRepo().SearchAsync(Public(new PropertyAdFilterDto { Search = "Bilgah" }));

        Assert.Equal(1, byTitle.TotalCount);
        Assert.Equal(1, byDistrict.TotalCount);
    }

    [Fact]
    public async Task Featured_filter_only_returns_featured()
    {
        using var env = new TestEnv();
        env.AddProperty(env.Merdakan, 100_000, 500, featured: true);
        env.AddProperty(env.Merdakan, 100_000, 500, featured: false);

        var result = await env.PropertyRepo().SearchAsync(Public(new PropertyAdFilterDto { Featured = true }));

        Assert.Equal(1, result.TotalCount);
    }
}
