using ArsaDuragi.Domain.Enums;

namespace ArsaDuragi.Application.DTOs.PropertyAd;

/// <summary>Owner-initiated status change: Sold, Archived, or Pending (resubmit after rejection/archiving).</summary>
public class PropertyAdStatusChangeDto
{
    public PropertyStatus Status { get; set; }
}

public class RejectPropertyAdDto
{
    public string Reason { get; set; } = string.Empty;
}

public class SetFeaturedDto
{
    public bool IsFeatured { get; set; }
}
