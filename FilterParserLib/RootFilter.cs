namespace FilterParserLib;

public record RootFilter (IEnumerable<Filter>? Filters, Operator Operator);