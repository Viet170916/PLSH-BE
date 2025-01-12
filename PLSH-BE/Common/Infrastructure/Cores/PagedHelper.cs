using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common.Enums;
using Common.Infrastructure.Extensions;

namespace Common.Infrastructure.Cores
{
    [ExcludeFromCodeCoverage]
    public static class PagedHelper
    {
        public static IQueryable<T> OrderByAndPaging<T>(IQueryable<T> queryable, int pageIndex, int pageSize,
            string sortField, SortOrderEnum ascending)
        {
            if (sortField != null)
            {
                queryable = OrderByExtension.OrderByPropertyOrField(queryable, sortField, ascending);
            }
            return queryable.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
    }
}
