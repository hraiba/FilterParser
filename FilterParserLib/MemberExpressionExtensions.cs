using System.Linq.Expressions;
using System.Reflection;

namespace FilterParserLib;

internal static class MemberExpressionExtensions
{
    internal static MethodCallExpression GetContains(this MemberExpression? property, ConstantExpression constant) =>
        Expression.Call(
            property,
            typeof(string).GetMethod("Contains", [typeof(string)])!,
            constant);

    internal static MethodCallExpression GetStartWith(this MemberExpression? property, ConstantExpression constant)
    {
        MethodInfo? startWithMethod =
            typeof(string).GetMethod("StartsWith", [typeof(string), typeof(StringComparison)]);
        MethodCallExpression constantLower =
            Expression.Call(constant, typeof(string).GetMethod("ToLower", Type.EmptyTypes)!);
        return Expression.Call(property, startWithMethod!, constantLower,
            Expression.Constant(StringComparison.OrdinalIgnoreCase));

    }
}
