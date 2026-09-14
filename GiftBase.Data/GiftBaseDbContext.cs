using System;
using GiftBase.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GiftBase.Data;

public class GiftBaseDbContext(DbContextOptions<GiftBaseDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GiftBaseDbContext).Assembly);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Person> Persons { get; set; }
    public DbSet<Gift> Gifts { get; set; }
    public DbSet<Occasion> Occasions { get; set; }
    public DbSet<ShareLink> ShareLinks { get; set; }
    public DbSet<GiftSuggestionQuota> GiftSuggestionQuotas { get; set; }
}
