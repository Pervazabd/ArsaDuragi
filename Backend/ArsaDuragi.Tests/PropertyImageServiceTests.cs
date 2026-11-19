using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ArsaDuragi.Tests;

public class PropertyImageServiceTests
{
    private static UploadFile File(string name = "a.jpg") => new(new MemoryStream(new byte[] { 1, 2, 3 }), name, 3);

    [Fact]
    public async Task First_uploaded_image_becomes_main()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500);
        env.LoginAs(env.Owner);

        var result = await env.ImageService().AddImagesAsync(ad.Id, new[] { File(), File() });

        Assert.Equal(2, result.Count);
        Assert.Single(await env.Db.PropertyImages.Where(i => i.IsMain).ToListAsync());
    }

    [Fact]
    public async Task Stranger_cannot_upload_or_delete_images()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500);
        env.LoginAs(env.Owner);
        var uploaded = await env.ImageService().AddImagesAsync(ad.Id, new[] { File() });

        env.LoginAs(env.Stranger);
        await Assert.ThrowsAsync<ForbiddenException>(() => env.ImageService().AddImagesAsync(ad.Id, new[] { File() }));
        await Assert.ThrowsAsync<ForbiddenException>(() => env.ImageService().DeleteImageAsync(ad.Id, uploaded[0].Id));
    }

    [Fact]
    public async Task Image_count_per_listing_is_limited()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500);
        env.LoginAs(env.Owner);

        var tooMany = Enumerable.Range(0, UploadLimits.MaxImagesPerProperty + 1).Select(_ => File()).ToList();

        await Assert.ThrowsAsync<BadRequestException>(() => env.ImageService().AddImagesAsync(ad.Id, tooMany));
    }

    [Fact]
    public async Task Set_main_keeps_exactly_one_main_and_delete_promotes_another()
    {
        using var env = new TestEnv();
        var ad = env.AddProperty(env.Merdakan, 100_000, 500);
        env.LoginAs(env.Owner);
        var images = await env.ImageService().AddImagesAsync(ad.Id, new[] { File(), File() });

        await env.ImageService().SetMainImageAsync(ad.Id, images[1].Id);
        Assert.Equal(images[1].Id, (await env.Db.PropertyImages.SingleAsync(i => i.IsMain)).Id);

        await env.ImageService().DeleteImageAsync(ad.Id, images[1].Id);
        var remaining = await env.Db.PropertyImages.ToListAsync();
        Assert.Single(remaining);
        Assert.True(remaining[0].IsMain);
        Assert.Single(env.Storage.Deleted); // the file was removed from storage
    }
}
