using System.Text.RegularExpressions;
using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.Exceptions;
using ArsaDuragi.Application.Interfaces.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ArsaDuragi.Infrastructure.FileStorage;

/// <summary>
/// Stores images on local disk under wwwroot/uploads/properties/{propertyId}/{random-guid}.{ext}.
/// Security rules: the uploaded file NAME is never used; the extension comes from the detected file signature;
/// size is enforced while streaming; the final path is verified to stay inside the upload root.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly Regex SafeFolder = new("^[a-z0-9]+(/[a-z0-9_-]+)*$", RegexOptions.Compiled);

    private readonly FileStorageOptions _options;
    private readonly IHttpContextAccessor _http;
    private readonly string _root;

    public LocalFileStorageService(IOptions<FileStorageOptions> options, IHostEnvironment env, IHttpContextAccessor http)
    {
        _options = options.Value;
        _http = http;
        _root = Path.GetFullPath(Path.Combine(env.ContentRootPath, _options.RootPath));
    }

    public async Task<StoredFileResult> SaveImageAsync(Stream content, string originalFileName, string folder, CancellationToken ct = default)
    {
        // 1) cheap checks first
        var claimedExtension = Path.GetExtension(originalFileName ?? string.Empty).ToLowerInvariant();
        if (!AllowedExtensions.Contains(claimedExtension))
            throw new BadRequestException("Only JPEG, PNG and WebP images are allowed.");

        var cleanFolder = folder.Trim('/').ToLowerInvariant();
        if (!SafeFolder.IsMatch(cleanFolder))
            throw new ArgumentException("Unsafe storage folder.", nameof(folder));

        // 2) check the real file signature (first bytes), not the name
        var header = new byte[ImageFileValidator.HeaderLength];
        var read = await ReadFullyAsync(content, header, ct);
        if (read == 0)
            throw new BadRequestException("The uploaded file is empty.");

        if (!ImageFileValidator.TryGetExtension(header.AsSpan(0, read), out var detectedExtension))
            throw new BadRequestException("The file is not a valid JPEG, PNG or WebP image.");

        if (ImageFileValidator.NormalizeExtension(claimedExtension) != detectedExtension)
            throw new BadRequestException("The file content does not match its extension.");

        // 3) store under a server-generated name
        var objectKey = $"{cleanFolder}/{Guid.NewGuid():N}{detectedExtension}";
        var fullPath = ResolvePath(objectKey);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        try
        {
            await using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await fs.WriteAsync(header.AsMemory(0, read), ct);
                long total = read;

                var buffer = new byte[81920];
                int n;
                while ((n = await content.ReadAsync(buffer.AsMemory(), ct)) > 0)
                {
                    total += n;
                    if (total > UploadLimits.MaxFileSizeBytes)
                        throw new BadRequestException($"Image is too large. Maximum size is {UploadLimits.MaxFileSizeBytes / (1024 * 1024)} MB.");

                    await fs.WriteAsync(buffer.AsMemory(0, n), ct);
                }
            }
        }
        catch
        {
            TryDelete(fullPath); // never keep a half-written or oversized file
            throw;
        }

        return new StoredFileResult(objectKey, $"{_options.RequestPath.TrimEnd('/')}/{objectKey}");
    }

    public Task DeleteAsync(string? objectKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(objectKey)) return Task.CompletedTask;

        string fullPath;
        try { fullPath = ResolvePath(objectKey); }
        catch (InvalidOperationException) { return Task.CompletedTask; } // a tampered key is ignored, never followed

        TryDelete(fullPath);

        // remove the per-listing folder when it became empty
        var dir = Path.GetDirectoryName(fullPath);
        try
        {
            if (dir is not null && dir != _root && Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any())
                Directory.Delete(dir);
        }
        catch (IOException) { /* not important */ }

        return Task.CompletedTask;
    }

    public string? GetPublicUrl(string? urlOrPath)
    {
        if (string.IsNullOrWhiteSpace(urlOrPath)) return null;
        if (urlOrPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            urlOrPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return urlOrPath; // demo images are absolute external URLs

        var path = urlOrPath.StartsWith('/') ? urlOrPath : "/" + urlOrPath;

        if (!string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
            return _options.PublicBaseUrl.TrimEnd('/') + path;

        var request = _http.HttpContext?.Request;
        return request is null ? path : $"{request.Scheme}://{request.Host}{request.PathBase}{path}";
    }

    /// <summary>Maps an object key to a disk path and makes sure it cannot escape the upload root (path traversal).</summary>
    private string ResolvePath(string objectKey)
    {
        var full = Path.GetFullPath(Path.Combine(_root, objectKey.Replace('/', Path.DirectorySeparatorChar)));
        if (!full.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Path escapes the upload root.");
        return full;
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch (IOException) { /* best effort */ }
        catch (UnauthorizedAccessException) { /* best effort */ }
    }

    private static async Task<int> ReadFullyAsync(Stream stream, byte[] buffer, CancellationToken ct)
    {
        var total = 0;
        while (total < buffer.Length)
        {
            var n = await stream.ReadAsync(buffer.AsMemory(total), ct);
            if (n == 0) break;
            total += n;
        }
        return total;
    }
}
