using GiftBase.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiftBase.Data.Configurations;

public class GiftSuggestionQuotaConfiguration : IEntityTypeConfiguration<GiftSuggestionQuota>
{
    public void Configure(EntityTypeBuilder<GiftSuggestionQuota> builder)
    {
        builder.HasKey(q => q.UserId);

        builder.Property(q => q.UserId)
            .ValueGeneratedNever();

        builder.Property(q => q.RequestCount)
            .IsRequired();

        builder.Property(q => q.WindowStartedAt)
            .IsRequired();

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<GiftSuggestionQuota>(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(q => q.ResetsAt);
    }
}
