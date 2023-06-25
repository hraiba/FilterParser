namespace FilterParser;

public class FilterService : IFilterService
{
    private readonly ITokenization _tokenization;
    
    public FilterService(ITokenization tokenization) 
    => _tokenization = tokenization;

    public FilterGroups Tokenize(Type type, string filter)
    {
        var filterValues = _tokenization.Tokenize(type, filter).ToArray(); 
        var x =  new FilterGroups(
            filterValues.Where(x => x.Operation == ConjunctionToken.And), 
            filterValues.Where(x => x.Operation == ConjunctionToken.Or));
        return x;
    }
}