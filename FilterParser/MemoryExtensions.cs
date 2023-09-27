namespace FilterParser;

public static class MemoryExtensions
{
    public static ReadOnlyMemory<ReadOnlyMemory<char>> Split(this ReadOnlySpan<char> source, char delimiter)
    {
        if (source.Length == 0 || source.IsWhiteSpace())
        {
            return Array.Empty<ReadOnlyMemory<char>>();
        }

        if (delimiter == '\0')
        {
            return new ReadOnlyMemory<char>[] {source.ToArray()};
        }

        var segments = new List<ReadOnlyMemory<char>>();

        var startIndex = 0;
        int delimiterIndex;

        while ((delimiterIndex = source[startIndex..].IndexOf(delimiter)) != -1)
        {
            segments.Add(source.Slice(startIndex, delimiterIndex).ToArray());
            startIndex += delimiterIndex + 1;
        }

        segments.Add(source[startIndex..].ToArray());

        return segments.ToArray();
    }

    public static IEnumerable<FilterValue> Tokenize(this ReadOnlyMemory<char> source, Type type)
    {
        if (source.IsEmpty)
        {
            return Enumerable.Empty<FilterValue>();
        }

        ReadOnlyMemory<char> memory = source.TrimStart('(').TrimEnd(')');
        if (memory.Span.Contains('&'))
        {
            return memory.TokenizeFilterValues(ConjunctionToken.And, type, '&').ToList();
        }

        if (memory.Span.Contains('|'))
        {
            return memory.TokenizeFilterValues(ConjunctionToken.Or, type, '|').ToList();
        }

        throw new Exception("Filter is not valid");
    }


    private static IEnumerable<FilterValue> TokenizeFilterValues(
        this ReadOnlyMemory<char> source,
        ConjunctionToken operation,
        Type type,
        char delimiter)
    {
        if (source.IsEmpty)
        {
            return Enumerable.Empty<FilterValue>();
        }

        ReadOnlyMemory<ReadOnlyMemory<char>> tokens = source.Span.Split(delimiter);
         List<Token> xx =  tokens
            .ToArray()
            .Select(token => CreateFilterValue( type, token.Span.Split('=').ToArray()))
            .ToList();
          return Array.Empty<FilterValue>();
    }


    private static Token CreateFilterValue(Type type, ReadOnlyMemory<char>[] t)
    {
        var properties = type.GetProperties();
        var propertyInfo =
            Array.Find(properties, x => x.Name.Equals(t[0].ToString(), StringComparison.OrdinalIgnoreCase));

        return propertyInfo is null
            ? new Token( "Unknown", null, null)
            : new Token(
                propertyInfo.Name,
                t[1].ToString(),
                propertyInfo.PropertyType);
    }
}