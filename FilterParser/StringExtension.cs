using System.Linq.Expressions;

namespace FilterParser;

public static class StringExtension
{
    public static IEnumerable<FilterValue> TokenizeAnd(this string? filter, Type type)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return Enumerable.Empty<FilterValue>();
        }

        var tokens = filter.Split("&", StringSplitOptions.RemoveEmptyEntries);
        return tokens
            .Select(t => t.Split("=", StringSplitOptions.RemoveEmptyEntries))
            .Select(t => FilterValue(type, t));
    }

    private static FilterValue FilterValue(Type type, string[] t)
    {
        var properties = type.GetProperties();
        var propertyInfo =
            Array.Find(properties, x => x.Name.Equals(t[0], StringComparison.OrdinalIgnoreCase));

        return propertyInfo is null
            ? new FilterValue(ConjunctionToken.And, "Unknown", null, null)
            : new FilterValue(ConjunctionToken.And, propertyInfo.Name, t[1], propertyInfo.PropertyType);
    }


    public static IEnumerable<FilterValue> TokenizeOr(this string? filter, Type type)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return Enumerable.Empty<FilterValue>();
        }

        return Enumerable.Empty<FilterValue>();
        // var tokens = filter.Split("|", StringSplitOptions.RemoveEmptyEntries);
        // return tokens
        //     .Select(t => t.Split("=", StringSplitOptions.RemoveEmptyEntries))
        //     .Select(t =>
        //     {
        //         if (Enum.TryParse(t[0].Trim('&').AsSpan(), ignoreCase: true, out SearchFilterType token))
        //         {
        //             return new FilterValue(token.ToString(), t[1].Trim('&'));
        //         }
        //         else
        //         {
        //             return new FilterValue(SearchFilterType.Unknown.ToString(), null);
        //         }
        //     });
    }


    public static IEnumerable<FilterValue> TokenizeNo(this string? filter, Type type)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return Enumerable.Empty<FilterValue>();
        }

        return Enumerable.Empty<FilterValue>();
        // var tokens = filter.Split(",", StringSplitOptions.RemoveEmptyEntries);
        // return tokens
        //     .Select(t => t.Split("=", StringSplitOptions.RemoveEmptyEntries))
        //     .Select(t => Enum.TryParse(t[0].AsSpan(), ignoreCase: true, out SearchFilterType token)
        //         ? new FilterValue(token.ToString(), t[1])
        //         : new FilterValue(SearchFilterType.Unknown.ToString(), null));
    }

    private static Expression<Func<T, bool>> WhereWithAndCondition<T>(
        IEnumerable<FilterValue> andFilters)
    {
        ParameterExpression param = Expression.Parameter(typeof(T), "p");
        Expression? body = null;
        foreach (FilterValue filter in andFilters)
        {
            if (filter.Token == "Unknown")
            {
                continue;
            }

            MemberExpression member = Expression.Property(param, filter.Token);
            ConstantExpression constant;
            constant = Expression.Constant(filter.Type!.BaseType == typeof(Enum) 
                ? Enum.Parse(filter.Type!, filter.Value!.ToString()!) 
                : Convert.ChangeType(filter.Value, filter.Type!));

            BinaryExpression expression = Expression.Equal(member, constant);
            body = body == null
                ? expression
                : Expression.AndAlso(body, expression);
        }

        return Expression.Lambda<Func<T, bool>>(body!, param);
    }


    private static Expression<Func<T, bool>> WhereWithNoCondition<T>(IEnumerable<FilterValue> orFilters)
    {
        ParameterExpression param = Expression.Parameter(typeof(T), "p");
        Expression? body = null;
        foreach (FilterValue filter in orFilters)
        {
            MemberExpression member = Expression.Property(param, filter.Token);
            ConstantExpression constant = Expression.Constant(filter.Value);
            BinaryExpression expression = Expression.Equal(member, constant);
            body = body == null
                ? expression
                : Expression.AndAlso(body, expression);
        }

        return Expression.Lambda<Func<T, bool>>(body!, param);
    }

    private static Expression<Func<T, bool>> WhereWithOrCondition<T>(IEnumerable<FilterValue> orFilters)
    {
        ParameterExpression param = Expression.Parameter(typeof(T), "p");
        Expression? body = null;
        foreach (FilterValue filter in orFilters)
        {
            if (filter.Token == "Unknown")
            {
                continue;
            }

            MemberExpression member = Expression.Property(param, filter.Token);
            ConstantExpression constant = Expression.Constant(filter.Value);
            BinaryExpression expression = Expression.Equal(member, constant);
            body = body == null
                ? expression
                : Expression.OrElse(body, expression);
        }

        return Expression.Lambda<Func<T, bool>>(body!, param);
    }

    public static IQueryable<T> Where<T>(this IQueryable<T> source, FilterGroups? filters)
    {
        if (filters is null)
        {
            return source;
        }

        return source.ConditionalWhere(filters.AndGroup.Any(),
            WhereWithAndCondition<T>(filters.AndGroup));
        // .ConditionalWhere(filters.OrGroup.Any(),
        //     WhereWithOrCondition<T>(filters.OrGroup))
        // .ConditionalWhere(filters.NoGroup.Any(),
        //     WhereWithNoCondition<T>(filters.NoGroup));
    }

    public static IQueryable<TSource> ConditionalWhere<TSource>(
        this IQueryable<TSource> source,
        bool condition,
        Expression<Func<TSource, bool>> predicate)
        => condition ? source.Where(predicate) : source;
}