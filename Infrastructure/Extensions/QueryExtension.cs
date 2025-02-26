namespace Infrastructure.Extensions
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public static class QueryExtension
    {
        /// <summary>
        /// Include related entities.
        /// </summary>
        /// <param name="source">The Source.</param>
        /// <param name="includeConfigurations">The Properties.</param>
        /// <returns>IQueryable&lt;TEntity&gt;.</returns>
        public static IQueryable<TEntity> PerformInclusions<TEntity, TSubEntity>(this IQueryable<TEntity> source, params (Expression<Func<TEntity, TSubEntity>> Include, Expression<Func<TSubEntity, object>>? ThenInclude)[] includeConfigurations) where TEntity : class
        {
            foreach (var (includeExpression, thenIncludeExpression) in includeConfigurations)
            {
                var query = source.Include(includeExpression);
                if (thenIncludeExpression != null) query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<TEntity, TSubEntity>)query.ThenInclude(thenIncludeExpression);
                source = query;
            }

            return source;
        }

        /// <summary>
        /// Paginate.
        /// </summary>
        /// <param name="source">The Source.</param>
        /// <param name="paginationFunc">The Properties.</param>
        /// <returns>IQueryable&lt;TEntity&gt;.</returns>
        public static IQueryable<TEntity> PerformPagination<TEntity>(this IQueryable<TEntity> source, Func<(int PageNumber, int PageSize)> paginationFunc) where TEntity : class
        {
            var (pageNumber, pageSize) = paginationFunc();

            return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }
    }
}