using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Data.Repository.Interfaces
{
  public interface IGenericRepository<T> where T : class
  {
    Task<T> GetById(int id);
    Task<IEnumerable<T>> GetAll();
    IEnumerable<T> Find(Expression<Func<T, bool>> expression);
    Task<int> Add(T entity);
    Task<int> Update(T entity);
    Task<int> AddRange(IEnumerable<T> entities);
    Task<int> Remove(T entity);
    Task<int> RemoveRange(IEnumerable<T> entities);
    IQueryable<T> GetAllQueryable();
    IQueryable<T> FindQueryable(Expression<Func<T, bool>> expression);
    Task<int> BulkUpdate(IList<T> items);
    Task<int> BulkInsert(IList<T> items);
    Task<int> BulkDelete(IList<T> items);
    IQueryable<T> GetAllQueryableNonTracking();
    IQueryable<T> FindQueryableNonTracking(Expression<Func<T, bool>> expression);
  }
}