using ArsaDuragi.Application.Common;

namespace ArsaDuragi.Application.Interfaces.Storage;

/// <summary>
/// Storage abstraction. Today the only implementation is local disk (wwwroot/uploads).
/// The Application layer only talks to this interface, so the storage can change without touching business logic.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Validates (size, extension, file signature) and stores an image.
    /// Throws BadRequestException when the file is not an allowed image.
    /// </summary>
    /// <param name="folder">Trusted, server-built relative folder such as "properties/12".</param>
    Task<StoredFileResult> SaveImageAsync(Stream content, string originalFileName, string folder, CancellationToken ct = default);

    /// <summary>Deletes a stored file. Silently ignores null keys and missing files.</summary>
    Task DeleteAsync(string? objectKey, CancellationToken ct = default);

    /// <summary>Turns a stored relative URL into a URL the browser can load (absolute URLs are returned unchanged).</summary>
    string? GetPublicUrl(string? urlOrPath);
}
