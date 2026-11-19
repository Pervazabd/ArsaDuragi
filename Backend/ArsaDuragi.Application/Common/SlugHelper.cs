using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ArsaDuragi.Application.Common;

public static class SlugHelper
{
    /// <summary>"Merdakan'da konut imarlı arsa" -> "merdakan-da-konut-imarli-arsa". Handles ə, ı, ö, ü, ş, ç, ğ.</summary>
    public static string Generate(string text, int maxLength = 80)
    {
        if (string.IsNullOrWhiteSpace(text)) return "listing";

        var prepared = text.Replace('ə', 'e').Replace('Ə', 'e').Replace('ı', 'i');
        var decomposed = prepared.Normalize(NormalizationForm.FormD);

        var sb = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        var lowered = sb.ToString().ToLowerInvariant();
        var slug = Regex.Replace(lowered, "[^a-z0-9]+", "-").Trim('-');

        if (slug.Length == 0) return "listing";
        if (slug.Length > maxLength) slug = slug[..maxLength].Trim('-');
        return slug.Length == 0 ? "listing" : slug;
    }
}
