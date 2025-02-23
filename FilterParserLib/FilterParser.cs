using System.ComponentModel;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FilterParserLib;

public static class FilterParser
{
    public static IQueryable<T> Filter<T>(IEnumerable<T>? dataSet, string? jsonString)
    {
        ArgumentNullException.ThrowIfNull(dataSet);

        if (!dataSet.Any())
        {
            return Enumerable.Empty<T>().AsQueryable();
        }

        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return dataSet.AsQueryable();
        }

        Search? rootFilter = GetFilter(jsonString);

        IQueryable<T> query = dataSet.AsQueryable();
        return Filter(query, rootFilter);
    }

    public static IQueryable<T> Filter<T>(this IQueryable<T> query, string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return query;
        }

        Search? filter = GetFilter(jsonString);
        return Filter(query, filter);
    }

    private static IQueryable<T> Filter<T>(IQueryable<T> query, Search? rootFilter)
    {
        if (rootFilter?.Filters == null || !rootFilter.Filters.Any())
        {
            return query;
        }

        var  filter = rootFilter.Operator switch
        {
            Operator.And => query.Where(GetAndFilterExpression<T>(rootFilter.Filters)),
            Operator.Or => query.Where(GetOrFilterExpression<T>(rootFilter.Filters)),
            _ => query
        };

        var order = rootFilter.OrderBy is null 
        ? filter 
        : filter.ApplyOrderBy(rootFilter.OrderBy);
        
        return order;
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

        MemberExpression property = Expression.Property(parameterExpression, filter.Field);
        var value = filter.GetValue(property.Type);
        if (value is null)
        {
            return Expression.Equal(property, Expression.Constant(null, property.Type));
        }

        if (property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            property = Expression.Property(property, "Value");
        }

        ConstantExpression constant = Expression.Constant(value);

        return filter.Operation switch
        {
            Operation.Equal => Expression.Equal(property, constant),
            Operation.NotEqual => Expression.NotEqual(property, constant),
            Operation.LessThan => Expression.LessThan(property, constant),
            Operation.LessThanOrEqual => Expression.LessThanOrEqual(property, constant),
            Operation.GreaterThan => Expression.GreaterThan(property, constant),
            Operation.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, constant),
            Operation.Contains => property.GetContains(constant),
            Operation.StartWith => property.GetStartWith(constant),
            _ => throw new ArgumentException($"Unsupported operation: {filter.Operation}"),
        };
    }

    private static Expression<Func<T, bool>> GetAndFilterExpression<T>(IEnumerable<Filter> filters)
    {
        if (!filters.Any())
        {
            Expression.Throw(Expression.Constant(new ArgumentException("Invalid filters")));
        }

        ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "x");
        Expression? andExpression = null;
        foreach (Filter filter in filters)
        {
            Expression filterExpression = BuildFilterExpression(filter, parameterExpression);
            andExpression =
                andExpression == null
                    ? filterExpression
                    : Expression.AndAlso(andExpression, filterExpression);
        }

        andExpression ??= Expression.Constant(false);
        return Expression.Lambda<Func<T, bool>>(andExpression, parameterExpression);
    }

    private static Expression<Func<T, bool>> GetOrFilterExpression<T>(IEnumerable<Filter> filters)
    {
        if (!filters.Any())
        {
            Expression.Throw(Expression.Constant(new ArgumentException("Invalid filters")));
        }

        ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "x");
        Expression? orExpression = null;
        foreach (Filter filter in filters)
        {
            Expression filterExpression = BuildFilterExpression(filter, parameterExpression);
            orExpression =
                orExpression == null
                    ? filterExpression
                    : Expression.OrElse(orExpression, filterExpression);
        }

        orExpression ??= Expression.Constant(false);
        return Expression.Lambda<Func<T, bool>>(orExpression, parameterExpression);
    }


private static Search? GetFilter(string jsonString)
    => JsonSerializer.Deserialize<Search>(jsonString,
        new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
                Converters = {new JsonStringEnumConverter()},
                AllowTrailingCommas = true,
        });
}
