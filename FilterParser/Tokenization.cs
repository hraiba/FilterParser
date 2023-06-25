namespace FilterParser;

public interface ITokenization
{
    IEnumerable<FilterValue> Tokenize(Type type, string filter, char delimiter = ';');
    IEnumerable<FilterValue> Tokenize(Type type, ReadOnlySpan<char> filter, char delimiter = ';');
}

public class Tokenization : ITokenization
{
    public IEnumerable<FilterValue> Tokenize(Type type, string filter, char delimiter = ';')
        => Tokenize(type, filter.AsSpan(), delimiter);

    public IEnumerable<FilterValue> Tokenize(Type type, ReadOnlySpan<char> filter, char delimiter = ';')
    {
        if (filter.IsEmpty || filter.IsWhiteSpace())
        {
            return Enumerable.Empty<FilterValue>();
        }

        var groups = filter.Split(delimiter);
        var filterValues = new List<FilterValue>();
        var array = groups.ToArray();
        foreach (var group in array)
        {
            filterValues.AddRange(group.Tokenize(type));
        }

        return filterValues;
    }
}