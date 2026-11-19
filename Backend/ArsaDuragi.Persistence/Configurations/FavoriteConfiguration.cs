using ArsaDuragi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArsaDuragi.Persistence.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("Favorites");
        builder.HasKey(f => f.Id);

        // One user cannot favorite the same listing twice (enforced by the database itself).
        builder.HasIndex(f => new { f.UserId, f.PropertyAdId }).IsUnique();

        builder.HasOne(f => f.User).WithMany(u => u.Favorites)
            .HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(f => !f.PropertyAd.IsDeleted);
    }
}
