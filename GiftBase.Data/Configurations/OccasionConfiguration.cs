using GiftBase.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiftBase.Data.Configurations;

public class OccasionConfiguration : IEntityTypeConfiguration<Occasion>
{
       public void Configure(EntityTypeBuilder<Occasion> builder)
       {
              builder.HasKey(o => o.Id);

              builder.Property(o => o.Type)
                     .IsRequired()
                     .HasConversion<string>()
                     .HasMaxLength(20);

              builder.Property(o => o.Title)
                     .IsRequired(false)
                     .HasMaxLength(100);

              builder.Property(o => o.Date)
                     .IsRequired();

              builder.Property(o => o.IsRecurring)
                     .IsRequired();

              builder.HasIndex(o => new { o.PersonId, o.Type })
                     .IsUnique()
                     .HasFilter("[Type] <> 'Custom'");

              builder.HasMany(o => o.Gifts)
                     .WithOne(g => g.Occasion)
                     .HasForeignKey(g => g.OccasionId)
                     .OnDelete(DeleteBehavior.ClientSetNull);
       }
}
