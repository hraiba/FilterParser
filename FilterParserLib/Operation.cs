
namespace FilterParserLib;

internal enum Operation
{
    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,
    Contains,
    StartWith,
}
 

 /*
    MemberExpression property = Expression.Property(parameterExpression, orderBy.Field);
    if(property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
    {
        property = Expression.Property(property, "Value");
    }
    var methodName = orderBy.Direction == Direction.Asc ? "OrderBy" : "OrderByDescending";
var lambda = Expression.Lambda<Func<T, TKey>>(property, parameterExpression); 

    var orderbyLambda = Expression.Lambda<Func<T, TKey>>(Expression.Constant(default(TKey)), parameterExpression);
    return orderbyLambda;
 
 */