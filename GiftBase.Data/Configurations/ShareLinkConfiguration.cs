using GiftBase.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiftBase.Data.Configurations;

public class ShareLinkConfiguration : IEntityTypeConfiguration<ShareLink>
{
    public void Configure(EntityTypeBuilder<ShareLink> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Token)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(s => s.Token)
            .IsUnique();

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.PersonId)
            .IsRequired();

        builder.Property(s => s.OccasionId)
            .IsRequired(false);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(s => s.ExpiresAt);
    }
}
