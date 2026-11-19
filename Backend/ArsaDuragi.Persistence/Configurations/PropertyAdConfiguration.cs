using ArsaDuragi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArsaDuragi.Persistence.Configurations;

public class PropertyAdConfiguration : IEntityTypeConfiguration<PropertyAd>
{
    public void Configure(EntityTypeBuilder<PropertyAd> builder)
    {
        builder.ToTable("PropertyAds");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Slug).IsRequired().HasMaxLength(160);
        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).IsRequired().HasMaxLength(4000);
        builder.Property(p => p.Address).IsRequired().HasMaxLength(300);
        builder.Property(p => p.RejectionReason).HasMaxLength(1000);
        builder.Property(p => p.ReviewedById).HasMaxLength(450);

        // Money and area must never be float/double.
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.Property(p => p.Area).HasPrecision(18, 2);

        // Soft delete: deleted listings disappear from every query automatically.
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Indexes for the columns the listing page filters and sorts by.
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => new { p.Status, p.IsFeatured });
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.Price);
        builder.HasIndex(p => p.Area);
        builder.HasIndex(p => p.ListedAt);
        builder.HasIndex(p => p.DistrictId);
        builder.HasIndex(p => p.CityId);
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.Zoning);
        builder.HasIndex(p => p.PropertyType);

        // Delete rules: never remove a user/city/district while listings reference them.
        builder.HasOne(p => p.User).WithMany(u => u.PropertyAds)
            .HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.City).WithMany(c => c.PropertyAds)
            .HasForeignKey(p => p.CityId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.District).WithMany(d => d.PropertyAds)
            .HasForeignKey(p => p.DistrictId).OnDelete(DeleteBehavior.Restrict);

        // Images and favorites belong to a listing and go away with it.
        builder.HasMany(p => p.Images).WithOne(i => i.PropertyAd)
            .HasForeignKey(i => i.PropertyAdId).OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Favorites).WithOne(f => f.PropertyAd)
            .HasForeignKey(f => f.PropertyAdId).OnDelete(DeleteBehavior.Cascade);
    }
}
