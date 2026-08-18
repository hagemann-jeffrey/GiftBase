using GiftBase.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftBase.Tests.Helper;

public class TestDbContextFactory : IDbContextFactory<GiftBaseDbContext>
{
    private readonly DbContextOptions<GiftBaseDbContext> _options;

    public TestDbContextFactory()
    {
        _options = new DbContextOptionsBuilder<GiftBaseDbContext>()
            .UseInMemoryDatabase($"GiftBaseTests_{Guid.NewGuid()}")
            .Options;
    }

    public GiftBaseDbContext CreateDbContext() => new(_options);
}
