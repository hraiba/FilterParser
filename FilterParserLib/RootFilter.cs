namespace FilterParserLib;

internal record RootFilter (IEnumerable<Filter>? Filters, Operator Operator);