
namespace FilterParser;

public interface IFilterService
{
    public FilterGroups Tokenize(Type type, string filter);
}