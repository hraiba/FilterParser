namespace FilterParser;

public record FilterGroups(IEnumerable<FilterValue> AndGroup, IEnumerable<FilterValue> OrGroup)
{
    public bool All(Func<object, bool> func)
    {
        return false;
    }
}