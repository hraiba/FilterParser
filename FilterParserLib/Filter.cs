using System.Text.Json;
using System.Text.Json.Serialization;

namespace FilterParserLib;

internal record Filter(
    string Field,
    Operation Operation,
    JsonElement Value,
    Operator Operator,
    IEnumerable<Filter>? Filters)
{
    public object? GetValue(Type type) => JsonSerializer.Deserialize(
        Value.GetRawText(),
        type,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } }
    );

}
