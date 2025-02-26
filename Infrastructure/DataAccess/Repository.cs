namespace Infrastructure.DataAccess
{
    using Core.Interfaces.DataAccess;
    using Infrastructure.Extensions;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using static Infrastructure.Extensions.QueryExtension;

    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        #region Properties
        protected DbContext DbContext { get; set; }
        protected DbSet<TEntity> Entity { get; set; }
        #endregion

        public Repository(DbContext context)
        {
            DbContext = context;
            DbContext.ChangeTracker.LazyLoadingEnabled = false;
            DbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            Entity = DbContext.Set<TEntity>();
        }

        #region Queries
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await Entity.AsNoTracking().ToListAsync();
        }

        public async Task<(IEnumerable<TEntity> Data, int TotalRecords)> GetAllAsync(
            Expression<Func<TEntity, bool>>? where = null,
            (Expression<Func<TEntity, object>> Include, Expression<Func<object, object>>? ThenInclude)[]? includeProperties = null,
            Func<(int PageNumber, int PageSize)>? paginationExpr = null)
        {
            IQueryable<TEntity> query = Entity.AsQueryable();
            if (includeProperties != null) query = query.PerformInclusions(includeProperties);
            if (where != null) query = query.Where(where);
            int totalRecords = await query.CountAsync();
            if (paginationExpr != null)query = query.PerformPagination(paginationExpr);
            var data = await query.AsNoTracking().ToListAsync();

            return (data, totalRecords);
        }

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> where, params (Expression<Func<TEntity, object>> Include, Expression<Func<object, object>>? ThenInclude)[]? includeProperties)
        {
            IQueryable<TEntity> query = Entity.AsQueryable();
            if (includeProperties != null) query = query.PerformInclusions(includeProperties);

            return await query.AsNoTracking().FirstOrDefaultAsync(where);
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> where)
        {
            IQueryable<TEntity> query = Entity.AsQueryable();

            return await query.AsNoTracking().AnyAsync(where);
        }
        #endregion

        #region Commands
        public async Task<bool> InsertAsync(TEntity entity)
        {
            await DbContext.AddAsync(entity);

            return await DbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> InsertAsync(IEnumerable<TEntity> entities)
        {
            await DbContext.AddRangeAsync(entities);

            return await DbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(TEntity entity)
        {
            DbContext.Entry(entity).State = EntityState.Modified;

            return await DbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(IEnumerable<TEntity> entities)
        {
            DbContext.Entry(entities).State = EntityState.Modified;

            return await DbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(TEntity entity)
        {
            DbContext.Remove(entity);

            return await DbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(IEnumerable<TEntity> entities)
        {
            DbContext.RemoveRange(entities);

            return await DbContext.SaveChangesAsync() > 0;
        }
        #endregion
    }
}