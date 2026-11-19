namespace ArsaDuragi.Application.Common;

/// <summary>
/// A framework-independent description of an uploaded file, so the Application layer
/// does not need to know about ASP.NET's IFormFile.
/// </summary>
public sealed record UploadFile(Stream Content, string FileName, long Length);

public sealed record StoredFileResult(string ObjectKey, string Url);

public sealed record AccessTokenResult(string Token, DateTime ExpiresAt);

public sealed record GeneratedRefreshToken(string RawToken, string TokenHash, DateTime ExpiresAt);

public sealed record IdentityOperationResult(bool Succeeded, IReadOnlyList<string> Errors);

public enum CredentialCheckResult
{
    Success,
    InvalidPassword,
    LockedOut
}
