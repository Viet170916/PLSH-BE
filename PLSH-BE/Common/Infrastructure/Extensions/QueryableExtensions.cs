using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class QueryableExtensions
    {
        private const int size = 10;
        public static IEnumerable<T> ToPaginatedList<T>(this IEnumerable<T> source, int pageNumber, int pageSize) where T : class
        {
            if (null == source)
            {
                return Enumerable.Empty<T>();
            }
            pageNumber = pageNumber == 0 ? 1 : pageNumber;
            pageSize = pageSize == 0 ? size : pageSize;
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        public async static Task<List<T>> ToPaginatedListAsync<T>(this IQueryable<T> source, int pageNumber, int pageSize) where T : class
        {
            if (null == source)
            {
                return null;
            }
            pageNumber = pageNumber == 0 ? 1 : pageNumber;
            pageSize = pageSize == 0 ? size : pageSize;
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            return await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);
        }

        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> records, string propertyOrFieldName, SortOrderEnum sortOrder)
        {
            string methodName = sortOrder == SortOrderEnum.ASC ? "OrderBy" : "OrderByDescending";
            var query = records.AsQueryable();
            ParameterExpression parameter = Expression.Parameter(query.ElementType, "p");

            MemberExpression memberAccess = Expression.Property(parameter, propertyOrFieldName);

            LambdaExpression orderByLambda = Expression.Lambda(memberAccess, parameter);

            MethodCallExpression result = Expression.Call(
                typeof(Queryable),
                methodName,
                new[] { query.ElementType, memberAccess.Type },
                query.Expression,
                Expression.Quote(orderByLambda));

            return query.Provider.CreateQuery<T>(result);
        }
    }
}
