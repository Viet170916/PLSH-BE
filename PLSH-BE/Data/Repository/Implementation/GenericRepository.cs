using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Data.DatabaseContext;
using Data.Repository.Interfaces;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Data.Repository.Implementation
{
    [ExcludeFromCodeCoverage]
    public class GenericRepository<T> : IDisposable, IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext Context;

        public GenericRepository(AppDbContext context)
        {
            Context = context;
        }

        public virtual async Task<int> BulkDelete(IList<T> items)
        {
            await Context.BulkDeleteAsync(items).ConfigureAwait(false);
            return await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public virtual async Task<int> Add(T entity)
        {
            await Context.Set<T>().AddAsync(entity);
            return await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public virtual async Task<int> AddRange(IEnumerable<T> entities)
        {
            await Context.Set<T>().AddRangeAsync(entities).ConfigureAwait(false);
            return await Context.SaveChangesAsync().ConfigureAwait(false);
        }
        
        public virtual IEnumerable<T> Find(Expression<Func<T, bool>> expression)
        {
            return Context.Set<T>().Where(expression);
        }

        public virtual async Task<IEnumerable<T>> GetAll()
        {
            return await Context.Set<T>().ToListAsync().ConfigureAwait(false);
        }

        public virtual IQueryable<T> GetAllQueryable()
        {
            IQueryable<T> query = Context.Set<T>();
            return query.AsQueryable();
        }
        public virtual IQueryable<T> GetAllQueryableNonTracking()
        {
            return Context.Set<T>().AsNoTracking();
        }

        public virtual IQueryable<T> FindQueryable(Expression<Func<T, bool>> expression)
        {
            return Context.Set<T>().Where(expression);
        }
        public virtual IQueryable<T> FindQueryableNonTracking(Expression<Func<T, bool>> expression)
        {
            return Context.Set<T>().AsNoTracking().Where(expression);
        }

        public virtual async Task<T> GetById(int id)
        {
            return await Context.Set<T>().FindAsync(id);
        }

        public virtual async Task<int> Remove(T entity)
        {
            Context.Set<T>().Remove(entity);
            return await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public virtual async Task<int> RemoveRange(IEnumerable<T> entities)
        {
            Context.Set<T>().RemoveRange(entities);
            return await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public virtual async Task<int> Update(T entity)
        {
            Context.Set<T>().Update(entity);
            return await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public virtual async Task<int> BulkUpdate(IList<T> items)
        {
            await Context.BulkUpdateAsync(items).ConfigureAwait(false);
            return await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public virtual async Task<int> BulkInsert(IList<T> items)
        {
            await Context.BulkInsertAsync(items).ConfigureAwait(false);
            return await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed && disposing)
            {
                    Context.Dispose();
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
