namespace FilterParserLib;

internal record Search(IEnumerable<Filter>? Filters, Operator Operator, OrderBy? OrderBy);

internal enum Direction
{
    Asc,
    Desc
}

internal record OrderBy(string Field, Direction Direction );

