using GiftBase.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiftBase.Data.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
       public void Configure(EntityTypeBuilder<Person> builder)
       {
              builder.HasKey(p => p.Id);

              builder.Property(p => p.FirstName)
                     .IsRequired()
                     .HasMaxLength(100);

              builder.Property(p => p.LastName)
                     .IsRequired()
                     .HasMaxLength(100);

              builder.Property(p => p.DateOfBirth)
                     .IsRequired(false);

              builder.Property(p => p.Relation)
                     .IsRequired()
                     .HasConversion<string>()
                     .HasMaxLength(20);
       }
}