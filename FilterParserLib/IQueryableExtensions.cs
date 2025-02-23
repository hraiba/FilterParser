using System.Linq.Expressions;

namespace FilterParserLib;

internal static class IQueryableExtensions
 {
    internal static IQueryable<T> ApplyOrderBy<T>(this IQueryable<T> query, OrderBy orderBy)
    {
        var propertyType = typeof(T).GetProperty(orderBy.Field)?.PropertyType
        ?? throw new InvalidOperationException($"Property {orderBy.Field} not found in {typeof(T).Name}");

        if(propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            propertyType = propertyType.GetGenericArguments()[0];
        }

        var methodName = orderBy.Direction == Direction.Asc ? "OrderBy" : "OrderByDescending";
        var method = typeof(Queryable).GetMethods()
        .First(m => m.Name == methodName && m.GetParameters().Length == 2);

        var genericMethod = method.MakeGenericMethod(typeof(T), propertyType);

        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        MemberExpression property = Expression.Property(parameter, orderBy.Field);
        var lambda = Expression.Lambda(property, parameter);

        if (genericMethod == null)
        {
            throw new InvalidOperationException("Failed to find the generic method.");
        }
        if (lambda == null)
        {

         throw new InvalidOperationException("Failed to create the lambda expression.");
        }

        return (IQueryable<T>?)genericMethod.Invoke(null, new object[] { query, lambda }) 
               ?? throw new InvalidOperationException("The method invocation returned null.");
    }
 }