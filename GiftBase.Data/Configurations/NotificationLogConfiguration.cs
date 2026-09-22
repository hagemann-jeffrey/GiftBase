using GiftBase.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiftBase.Data.Configurations;

public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Kind)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(30);

        builder.Property(n => n.PeriodKey)
               .IsRequired()
               .HasMaxLength(10);

        builder.Property(n => n.SentAtUtc)
               .IsRequired();

        builder.HasIndex(n => new { n.UserId, n.Kind, n.PeriodKey })
               .IsUnique();

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(n => n.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
