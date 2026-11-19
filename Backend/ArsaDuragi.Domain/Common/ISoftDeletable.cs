namespace ArsaDuragi.Domain.Common;

/// <summary>Entities that are never physically removed; they are flagged instead.</summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}
