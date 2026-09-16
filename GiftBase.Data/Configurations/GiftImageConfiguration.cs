using GiftBase.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiftBase.Data.Configurations;

public class GiftImageConfiguration : IEntityTypeConfiguration<GiftImage>
{
    public void Configure(EntityTypeBuilder<GiftImage> builder)
    {
        builder.HasKey(i => i.GiftId);

        builder.Property(i => i.GiftId)
            .ValueGeneratedNever();

        builder.Property(i => i.ContentType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Content)
            .IsRequired();

        builder.HasOne<Gift>()
            .WithOne()
            .HasForeignKey<GiftImage>(i => i.GiftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
