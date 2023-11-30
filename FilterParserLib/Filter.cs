using System.Text.Json;

namespace FilterParserLib;

public  record Filter(string Field, Keyword Keyword, JsonElement Value, Operator Operator, IEnumerable<Filter>? Filters);