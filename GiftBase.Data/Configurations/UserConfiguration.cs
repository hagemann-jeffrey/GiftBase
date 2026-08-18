using GiftBase.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiftBase.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
       public void Configure(EntityTypeBuilder<User> builder)
       {
              builder.HasKey(u => u.Id);

              builder.Property(u => u.Email)
                     .IsRequired()
                     .HasMaxLength(100);

              builder.Property(u => u.IsEmailVerified)
                     .IsRequired();

              builder.Property(u => u.PasswordHash)
                     .IsRequired();

              builder.Property(u => u.VerificationToken)
              .HasMaxLength(100)
              .IsRequired(false);

              builder.Property(u => u.TokenExpiresAt)
                     .IsRequired(false);

              builder.HasIndex(u => u.Email)
                     .IsUnique();
       }
}