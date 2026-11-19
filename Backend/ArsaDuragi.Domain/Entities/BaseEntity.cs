namespace ArsaDuragi.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }

    /// <summary>Set automatically by AppDbContext.SaveChanges (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Set automatically by AppDbContext.SaveChanges (UTC).</summary>
    public DateTime? UpdatedAt { get; set; }
}
