namespace FilterParser;

public class FilterService : IFilterService
{
    public FilterGroups Tokenize(Type type, string filter)
    {
        var tokens = filter.Split(";", StringSplitOptions.RemoveEmptyEntries);
        var andGroup = tokens
            .Where(t => t.StartsWith("and(")).Select(t => t[4..^1])
            .FirstOrDefault()
            .TokenizeAnd(type);
        var orGroup = tokens
            .Where(t => t.StartsWith("or("))
            .Select(t => t[3..^1])
            .FirstOrDefault()
            .TokenizeOr(type);
        var noGroup = tokens
            .Where(t => t.StartsWith("no(")).Select(t => t[3..^1])
            .FirstOrDefault()
            .TokenizeNo(type);

        return new FilterGroups(andGroup, orGroup, noGroup);
    }
}