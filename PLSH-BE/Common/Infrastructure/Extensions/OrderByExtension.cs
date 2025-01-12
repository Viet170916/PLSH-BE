using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Common.Enums;

namespace Common.Infrastructure.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class OrderByExtension
    {
        public static IQueryable<T> OrderByPropertyOrField<T>(IQueryable<T> query, string propertyOrFieldName,
            SortOrderEnum sortOrder)
        {
            var elementType = typeof(T);
            var orderByMethodName = sortOrder.Equals(SortOrderEnum.DESC) ? "OrderByDescending" : "OrderBy";

            var parameterExpression = Expression.Parameter(elementType);
            var propertyOrFieldExpression = Expression.PropertyOrField(parameterExpression, propertyOrFieldName);
            var selector = Expression.Lambda(propertyOrFieldExpression, parameterExpression);

            var orderByExpression = Expression.Call(typeof(Queryable), orderByMethodName,
                new[] { elementType, propertyOrFieldExpression.Type }, query.Expression, selector);

            return query.Provider.CreateQuery<T>(orderByExpression);
        }
    }
}
