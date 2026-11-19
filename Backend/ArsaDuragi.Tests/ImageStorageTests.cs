using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.Exceptions;
using ArsaDuragi.Infrastructure.FileStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ArsaDuragi.Tests;

public class ImageStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "arsa-tests-" + Guid.NewGuid().ToString("N"));
    private readonly LocalFileStorageService _storage;

    private static readonly byte[] Jpeg = { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0x10, (byte)'J', (byte)'F', (byte)'I', (byte)'F', 0, 1 };
    private static readonly byte[] Png = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0 };
    private static readonly byte[] Webp = { (byte)'R', (byte)'I', (byte)'F', (byte)'F', 1, 0, 0, 0, (byte)'W', (byte)'E', (byte)'B', (byte)'P' };

    public ImageStorageTests()
    {
        Directory.CreateDirectory(_root);
        var env = new Mock<IHostEnvironment>();
        env.SetupGet(e => e.ContentRootPath).Returns(_root);
        _storage = new LocalFileStorageService(Options.Create(new FileStorageOptions()), env.Object, new HttpContextAccessor());
    }

    [Fact]
    public async Task Valid_jpeg_png_and_webp_are_stored_under_a_generated_name()
    {
        foreach (var (name, bytes, ext) in new[] { ("photo.jpg", Jpeg, ".jpg"), ("photo.png", Png, ".png"), ("photo.webp", Webp, ".webp") })
        {
            var result = await _storage.SaveImageAsync(new MemoryStream(bytes), name, "properties/7");

            Assert.StartsWith("properties/7/", result.ObjectKey);
            Assert.EndsWith(ext, result.ObjectKey);
            Assert.DoesNotContain("photo", result.ObjectKey); // the client's file name is never used
            Assert.StartsWith("/uploads/properties/7/", result.Url);
            Assert.True(File.Exists(Path.Combine(_root, "wwwroot", "uploads", result.ObjectKey.Replace('/', Path.DirectorySeparatorChar))));
        }
    }

    [Fact]
    public async Task Executable_renamed_to_jpg_is_rejected_by_signature()
    {
        var exe = new byte[] { (byte)'M', (byte)'Z', 0x90, 0, 3, 0, 0, 0, 4, 0, 0, 0 };

        await Assert.ThrowsAsync<BadRequestException>(() => _storage.SaveImageAsync(new MemoryStream(exe), "evil.jpg", "properties/7"));
    }

    [Fact]
    public async Task Script_with_image_extension_is_rejected()
    {
        var html = System.Text.Encoding.UTF8.GetBytes("<script>alert(1)</script>");

        await Assert.ThrowsAsync<BadRequestException>(() => _storage.SaveImageAsync(new MemoryStream(html), "x.png", "properties/7"));
    }

    [Theory]
    [InlineData("shell.php")]
    [InlineData("doc.pdf")]
    [InlineData("noextension")]
    [InlineData("image.gif")]
    public async Task Disallowed_extensions_are_rejected(string name)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _storage.SaveImageAsync(new MemoryStream(Jpeg), name, "properties/7"));
    }

    [Fact]
    public async Task Extension_must_match_real_content()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _storage.SaveImageAsync(new MemoryStream(Png), "image.jpg", "properties/7"));
    }

    [Fact]
    public async Task Empty_file_is_rejected()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _storage.SaveImageAsync(new MemoryStream(), "a.jpg", "properties/7"));
    }

    [Fact]
    public async Task Oversized_file_is_rejected_and_nothing_is_left_on_disk()
    {
        var big = new byte[UploadLimits.MaxFileSizeBytes + 1024];
        Array.Copy(Jpeg, big, Jpeg.Length);

        await Assert.ThrowsAsync<BadRequestException>(() => _storage.SaveImageAsync(new MemoryStream(big), "big.jpg", "properties/7"));

        var folder = Path.Combine(_root, "wwwroot", "uploads", "properties", "7");
        Assert.True(!Directory.Exists(folder) || !Directory.EnumerateFiles(folder).Any());
    }

    [Fact]
    public async Task Path_traversal_folder_is_refused()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _storage.SaveImageAsync(new MemoryStream(Jpeg), "a.jpg", "../../etc"));
    }

    [Fact]
    public async Task Delete_removes_file_and_ignores_tampered_keys()
    {
        var saved = await _storage.SaveImageAsync(new MemoryStream(Jpeg), "a.jpg", "properties/9");
        var path = Path.Combine(_root, "wwwroot", "uploads", saved.ObjectKey.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(path));

        await _storage.DeleteAsync(saved.ObjectKey);
        Assert.False(File.Exists(path));

        await _storage.DeleteAsync("../../../../etc/passwd"); // must not throw and must not touch anything outside the root
        await _storage.DeleteAsync(null);
    }

    [Theory]
    [InlineData("https://images.unsplash.com/x.jpg", "https://images.unsplash.com/x.jpg")]
    [InlineData("/uploads/properties/1/a.jpg", "/uploads/properties/1/a.jpg")]
    public void Public_url_keeps_absolute_urls_and_relative_paths_without_request(string input, string expected)
    {
        Assert.Equal(expected, _storage.GetPublicUrl(input));
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, true); } catch (IOException) { }
    }
}
