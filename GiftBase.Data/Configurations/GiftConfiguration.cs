using GiftBase.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiftBase.Data.Configurations;

public class GiftConfiguration : IEntityTypeConfiguration<Gift>
{
    public void Configure(EntityTypeBuilder<Gift> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Title)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(g => g.Note)
               .IsRequired(false)
               .HasMaxLength(1000);

        builder.Property(g => g.Link)
               .IsRequired(false)
               .HasMaxLength(2000);

        builder.Property(g => g.Price)
               .IsRequired(false)
               .HasPrecision(18, 2);

        builder.Property(g => g.Status)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.Property(g => g.OccasionLabel)
               .IsRequired(false)
               .HasMaxLength(100);

        builder.Property(g => g.OccasionYear)
               .IsRequired(false);
    }
}