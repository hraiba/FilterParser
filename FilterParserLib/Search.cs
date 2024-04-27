namespace FilterParserLib;

internal record Search(IEnumerable<Filter>? Filters, Operator Operator);
