namespace ArsaDuragi.Application.Common;

public sealed class ModerationOptions
{
    public const string SectionName = "Moderation";

    /// <summary>
    /// When true, listings created by normal users start as Pending and must be approved by an Admin.
    /// Listings created by an Admin are always published immediately.
    /// </summary>
    public bool RequireApproval { get; set; } = true;

    /// <summary>
    /// When true (default) only an Admin can create, edit, delete and manage images of listings.
    /// Normal users register only to browse and save favorites. Set to false to let customers post
    /// their own listings (they then go through RequireApproval moderation).
    /// </summary>
    public bool AdminOnlyPosting { get; set; } = true;
}
