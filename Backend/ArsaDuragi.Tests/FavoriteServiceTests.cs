using ArsaDuragi.Application.Exceptions;
using ArsaDuragi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ArsaDuragi.Tests;

public class FavoriteServiceTests
{
    [Fact]
    public async Task Add_favorite_succeeds()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500);
        env.LoginAs(env.Stranger);

        var favorite = await env.FavoriteService().AddAsync(ad.Id);

        Assert.Equal(ad.Id, favorite.PropertyAdId);
        Assert.Single(await env.Db.Favorites.ToListAsync());
    }

    [Fact]
    public async Task Duplicate_favorite_is_a_conflict()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500);
        env.LoginAs(env.Stranger);
        var service = env.FavoriteService();

        await service.AddAsync(ad.Id);

        await Assert.ThrowsAsync<ConflictException>(() => service.AddAsync(ad.Id));
        Assert.Single(await env.Db.Favorites.ToListAsync());
    }

    [Fact]
    public async Task Cannot_favorite_a_pending_listing()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500, PropertyStatus.Pending);
        env.LoginAs(env.Stranger);

        await Assert.ThrowsAsync<NotFoundException>(() => env.FavoriteService().AddAsync(ad.Id));
    }

    [Fact]
    public async Task Remove_favorite_and_users_only_see_their_own()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500);

        env.LoginAs(env.Stranger);
        await env.FavoriteService().AddAsync(ad.Id);

        env.LoginAs(env.Owner);
        Assert.Empty((await env.FavoriteService().GetMineAsync(1, 10)).Items);         // owner sees nothing of the stranger's favorites
        await Assert.ThrowsAsync<NotFoundException>(() => env.FavoriteService().RemoveAsync(ad.Id)); // and cannot remove them

        env.LoginAs(env.Stranger);
        Assert.Single((await env.FavoriteService().GetMineAsync(1, 10)).Items);
        await env.FavoriteService().RemoveAsync(ad.Id);
        Assert.Empty(await env.Db.Favorites.ToListAsync());
    }

    [Fact]
    public async Task Favorites_require_authentication()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500);

        await Assert.ThrowsAsync<UnauthorizedException>(() => env.FavoriteService().AddAsync(ad.Id));
    }
}
