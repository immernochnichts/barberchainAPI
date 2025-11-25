using Microsoft.EntityFrameworkCore;

namespace barberchainAPI.Functional
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string prop)
        {
            if (string.IsNullOrWhiteSpace(prop)) return query;
            return query.OrderBy(x => EF.Property<object>(x, prop));
        }

        public static IQueryable<T> OrderByDescendingDynamic<T>(this IQueryable<T> query, string prop)
        {
            if (string.IsNullOrWhiteSpace(prop)) return query;
            return query.OrderByDescending(x => EF.Property<object>(x, prop));
        }
    }
}
