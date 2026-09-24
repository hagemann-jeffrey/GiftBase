using GiftBase.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GiftBase.Data;

public static class QueryableExtensions
{
    public static async Task<T> SingleOrNotFoundAsync<T>(this IQueryable<T> query, string message)
        where T : class =>
        await query.SingleOrDefaultAsync() ?? throw new NotFoundException(message);
}
