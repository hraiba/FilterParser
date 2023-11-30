using System.Text.Json;

namespace FilterParserLib;

public  class Filter
{
    public string Field { get; set; }
    public Operator Operator { get; set; }
    public JsonElement Value { get; set; }
    public Logic Logic { get; set; }
    public List<Filter> Filters { get; set; }
}