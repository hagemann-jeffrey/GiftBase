using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GiftBase.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddGiftBaseData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<GiftBaseDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("GiftBase"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 6,
                    maxRetryDelay: TimeSpan.FromSeconds(15),
                    errorNumbersToAdd: null)));

        return services;
    }
}
