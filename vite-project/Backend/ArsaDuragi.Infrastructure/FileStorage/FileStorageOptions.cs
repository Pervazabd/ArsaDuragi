namespace ArsaDuragi.Infrastructure.FileStorage;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>Relative to the content root. Keep the default so ASP.NET's static files serve it at /uploads.</summary>
    public string RootPath { get; set; } = "wwwroot/uploads";

    /// <summary>URL prefix under which the files are served.</summary>
    public string RequestPath { get; set; } = "/uploads";

    /// <summary>Optional, e.g. https://api.arsaduragi.az. When empty the URL is built from the current request.</summary>
    public string? PublicBaseUrl { get; set; }
}
