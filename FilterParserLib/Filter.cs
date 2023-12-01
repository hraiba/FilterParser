using System.Text.Json;

namespace FilterParserLib;

internal record Filter(
    string Field,
    Operation Operation,
    JsonElement Value,
    Operator Operator,
    IEnumerable<Filter>? Filters);