using System.ComponentModel;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FilterParserLib;

public static class FilterParser
{
    public static IQueryable<T> ApplyFilter<T>(IEnumerable<T> dataSet, string? jsonString)
    {
        if (dataSet == null)
        {
            throw new ArgumentNullException(nameof(dataSet));
        }

        if (!dataSet.Any())
        {
            return Enumerable.Empty<T>().AsQueryable();
        }

        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return dataSet.AsQueryable();
        }

        var rootFilter = GetFilter(jsonString);

        var query = dataSet.AsQueryable();
        return ApplyFilter(query, rootFilter);
    }

    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return query;
        }

        var filter = GetFilter(jsonString);
        return ApplyFilter(query, filter);
    }

    private static IQueryable<T> ApplyFilter<T>(IQueryable<T> query, RootFilter? rootFilter)
    {
        if (rootFilter?.Filters == null || !rootFilter.Filters.Any())
        {
            return query;
        }

        return rootFilter.Operator switch
        {
            Operator.And => query.Where(GetAndFilterExpression<T>(rootFilter.Filters)),
            Operator.Or => query.Where(GetOrFilterExpression<T>(rootFilter.Filters)),
            _ => query
        };
    }

    private static Expression BuildFilterExpression(Filter filter, ParameterExpression parameterExpression)
    {
        if (filter.Filters != null && filter.Filters.Any())
        {
            return filter.Operator switch
            {
                Operator.And => filter.Filters.Select(f => BuildFilterExpression(f, parameterExpression))
                    .Aggregate(Expression.AndAlso),
                Operator.Or => filter.Filters.Select(f => BuildFilterExpression(f, parameterExpression))
                    .Aggregate(Expression.OrElse),
                _ => throw new InvalidEnumArgumentException()
            };
        }

        var value = GetValue(filter);
        if (value is null)
        {
            return Expression.Throw(Expression.Constant(new ArgumentException($"Invalid value: {filter.Value}")));
        }

        var property = Expression.Property(parameterExpression, filter.Field);
        var constant = Expression.Constant(value);

        return filter.Operation switch
        {
            Operation.Equal => Expression.Equal(property, constant),
            Operation.NotEqual => Expression.NotEqual(property, constant),
            Operation.LessThan => Expression.LessThan(property, constant),
            Operation.LessThanOrEqual => Expression.LessThanOrEqual(property, constant),
            Operation.GreaterThan => Expression.GreaterThan(property, constant),
            Operation.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, constant),
            Operation.Contains => GetContains(property, constant),
            Operation.StartWith => GetStartWith(property, constant),
            _ => throw new ArgumentException($"Unsupported operation: {filter.Operation}"),
        };
    }

    private static Expression<Func<T, bool>> GetAndFilterExpression<T>(IEnumerable<Filter> filters)
    {
        if (!filters.Any())
        {
            Expression.Throw(Expression.Constant(new ArgumentException("Invalid filters")));
        }

        var parameterExpression = Expression.Parameter(typeof(T), "x");
        Expression andExpression = filters
                                       .Select(filter => BuildFilterExpression(filter, parameterExpression))
                                       .Aggregate<Expression?, Expression?>(
                                           null,
                                           (current, filterExpression) =>
                                               current == null
                                                   ? filterExpression
                                                   : Expression.AndAlso(current, filterExpression!))
                                   ?? Expression.Constant(false);

        return Expression.Lambda<Func<T, bool>>(andExpression, parameterExpression);
    }

    private static Expression<Func<T, bool>> GetOrFilterExpression<T>(IEnumerable<Filter> filters)
    {
        if (!filters.Any())
        {
            Expression.Throw(Expression.Constant(new ArgumentException("Invalid filters")));
        }

        var parameterExpression = Expression.Parameter(typeof(T), "x");
        Expression orExpression = filters.Select(filter => BuildFilterExpression(filter, parameterExpression))
                                      .Aggregate<Expression?, Expression?>(null, (current, filterExpression) =>
                                          current == null
                                              ? filterExpression
                                              : Expression.OrElse(current, filterExpression!))
                                  ?? Expression.Constant(false);

        return Expression.Lambda<Func<T, bool>>(orExpression, parameterExpression);
    }

    private static MethodCallExpression GetContains(MemberExpression? property, ConstantExpression constant)
    {
        return Expression.Call(
            property,
            typeof(string).GetMethod("Contains", new[] {typeof(string)})!,
            constant);
    }

    private static MethodCallExpression GetStartWith(MemberExpression? property, ConstantExpression constant)
    {
        var startWithMethod =
            typeof(string).GetMethod("StartsWith", new[] {typeof(string), typeof(StringComparison)});
        var constantLower = Expression.Call(constant, typeof(string).GetMethod("ToLower", Type.EmptyTypes)!);
        return Expression.Call(property, startWithMethod!, constantLower,
            Expression.Constant(StringComparison.OrdinalIgnoreCase));
    }

    private static object? GetValue(Filter? filter)
    {
        if (filter?.Value == null || string.IsNullOrWhiteSpace(filter.Value.ToString()))
        {
            return null;
        }

        return filter.Value.ValueKind switch
        {
            JsonValueKind.Number => GetNumber(filter.Value),
            JsonValueKind.String => filter.Value.GetString(),
            JsonValueKind.True => filter.Value.GetBoolean(),
            JsonValueKind.False => filter.Value.GetBoolean(),
            _ => null
        };
    }

    private static object GetNumber(JsonElement filterValue)
    {
        if (filterValue.TryGetInt32(out var @int))
        {
            return @int;
        }

        if (filterValue.TryGetInt64(out var int64))
        {
            return int64;
        }

        if (filterValue.TryGetDouble(out var @double))
        {
            return @double;
        }

        return filterValue.GetDecimal();
    }

    private static RootFilter? GetFilter(string jsonString)
    {
        return JsonSerializer.Deserialize<RootFilter>(jsonString, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            Converters = {new JsonStringEnumConverter()}
        });
    }
}