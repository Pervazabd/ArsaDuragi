using ArsaDuragi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArsaDuragi.Persistence.Configurations;

public class PropertyImageConfiguration : IEntityTypeConfiguration<PropertyImage>
{
    public void Configure(EntityTypeBuilder<PropertyImage> builder)
    {
        builder.ToTable("PropertyImages");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ImageUrl).IsRequired().HasMaxLength(1000);
        builder.Property(i => i.ObjectKey).HasMaxLength(500);
        builder.HasIndex(i => i.PropertyAdId);

        // Same filter as the parent, so EF does not warn about required navigations with filters.
        builder.HasQueryFilter(i => !i.PropertyAd.IsDeleted);
    }
}
