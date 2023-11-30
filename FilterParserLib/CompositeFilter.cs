using System.ComponentModel;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FilterParserLib;

public abstract class CompositeFilter<T>
{
    public static IQueryable<T> ApplyFilter(List<T> dataSet, string jsonStrring)
    {
        var rootFilter = JsonSerializer.Deserialize<RootFilter>(jsonStrring, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            Converters = {new JsonStringEnumConverter()}
        });

        var query = dataSet.AsQueryable();
        return ApplyFilter(query, rootFilter);
    }

    public static IQueryable<T> ApplyFilter(IQueryable<T> query, RootFilter? rootFilter)
    {
        if (rootFilter?.Filters == null || !rootFilter.Filters.Any())
        {
            return query;
        }

        return rootFilter.Logic switch
        {
            Logic.And => query.Where(GetAndFilterExpression(rootFilter.Filters)),
            Logic.Or => query.Where(GetOrFilterExpression(rootFilter.Filters)),
            _ => query
        };
    }

    private static Expression BuildFilterExpression(Filter filter, ParameterExpression parameterExpression)
    {
        if (filter.Filters != null && filter.Filters.Any())
        {
            return filter.Logic switch
            {
                Logic.And => filter.Filters.Select(f => BuildFilterExpression(f, parameterExpression))
                    .Aggregate(Expression.AndAlso),
                Logic.Or => filter.Filters.Select(f => BuildFilterExpression(f, parameterExpression))
                    .Aggregate(Expression.OrElse),
                _ => throw new InvalidEnumArgumentException()
            };
        }

        var value = GetValue(filter);
        if (value is null)
        {
            return Expression.Throw(Expression.Constant(new ArgumentException($"Invalid value: {filter?.Value}")));
        }

        var property = Expression.Property(parameterExpression, filter.Field);
        var constant = Expression.Constant(value);

        return filter.Operator switch
        {
            Operator.Equal => Expression.Equal(property, constant),
            Operator.NotEqual => Expression.NotEqual(property, constant),
            Operator.LessThan => Expression.LessThan(property, constant),
            Operator.LessThanOrEqual => Expression.LessThanOrEqual(property, constant),
            Operator.GreaterThan => Expression.GreaterThan(property, constant),
            Operator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, constant),
            Operator.Contains => GetContains(property, constant),
            Operator.StartWith => GetStartWith(property, constant),
            _ => throw new ArgumentException($"Unsported operator: {filter.Operator}"),
        };
    }

    private static Expression<Func<T, bool>> GetAndFilterExpression(List<Filter> filters)
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

    private static Expression<Func<T, bool>> GetOrFilterExpression(List<Filter> filters)
    {
        if (!filters.Any())
        {
            Expression.Throw(Expression.Constant(new ArgumentException("Invalid filters")));
        }

        var parameterExpression = Expression.Parameter(typeof(T), "x");
        Expression? orExpression = null;
        foreach (var filter in filters)
        {
            var filterExpression = BuildFilterExpression(filter, parameterExpression);
            if (filterExpression != null)
            {
                if (orExpression == null)
                {
                    orExpression = filterExpression;
                }
                else
                {
                    orExpression = Expression.OrElse(orExpression, filterExpression);
                }
            }
        }

        if (orExpression == null)
        {
            orExpression = Expression.Constant(false);
        }

        return Expression.Lambda<Func<T, bool>>(orExpression, parameterExpression);
    }

    private static MethodCallExpression GetContains(MemberExpression? property, ConstantExpression constant)
    {
        return Expression.Call(
            property,
            typeof(string).GetMethod("Contains", new[] {typeof(string)}),
            constant);
    }

    private static MethodCallExpression GetStartWith(MemberExpression? property, ConstantExpression constant)
    {
        var startWithMethod =
            typeof(string).GetMethod("StartsWith", new[] {typeof(string), typeof(StringComparison)});
        var constantLower = Expression.Call(constant, typeof(string).GetMethod("ToLower", Type.EmptyTypes));
        return Expression.Call(property, startWithMethod, constantLower,
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
            JsonValueKind.Number => GetNumber(filter),
            JsonValueKind.String => filter.Value.GetString(),
            JsonValueKind.True => filter.Value.GetBoolean(),
            JsonValueKind.False => filter.Value.GetBoolean(),
            _ => null
        };
    }

    private static object GetNumber(Filter filter)
    {
        if (filter.Value.TryGetInt32(out var @int))
        {
            return @int;
        }

        if (filter.Value.TryGetInt64(out var int64))
        {
            return int64;
        }

        if (filter.Value.TryGetDouble(out var @double))
        {
            return @double;
        }

        return filter.Value.GetDecimal();
    }
}