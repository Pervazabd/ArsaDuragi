namespace ArsaDuragi.Infrastructure.FileStorage;

/// <summary>
/// Looks at the first bytes of the file ("magic numbers") to find out what it REALLY is.
/// A file called photo.jpg that starts with "MZ" (an .exe) or "&lt;script" is rejected.
/// </summary>
public static class ImageFileValidator
{
    public const int HeaderLength = 12;

    /// <summary>Returns ".jpg", ".png" or ".webp" if the header matches one of them.</summary>
    public static bool TryGetExtension(ReadOnlySpan<byte> header, out string extension)
    {
        extension = string.Empty;

        // JPEG: FF D8 FF
        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            extension = ".jpg";
            return true;
        }

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (header.Length >= 8 &&
            header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
            header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
        {
            extension = ".png";
            return true;
        }

        // WebP: "RIFF" <4 size bytes> "WEBP"
        if (header.Length >= 12 &&
            header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F' &&
            header[8] == 'W' && header[9] == 'E' && header[10] == 'B' && header[11] == 'P')
        {
            extension = ".webp";
            return true;
        }

        return false;
    }

    /// <summary>".jpeg" and ".jpg" are the same format.</summary>
    public static string NormalizeExtension(string extension)
    {
        var ext = extension.ToLowerInvariant();
        return ext == ".jpeg" ? ".jpg" : ext;
    }
}
