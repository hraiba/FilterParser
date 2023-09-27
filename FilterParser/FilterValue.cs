namespace FilterParser;

public record FilterValue(
    ConjunctionToken Operation,  
    Token Left,
    Token Right);

    public record Token(
        string TokenName,
        object? Value,
        Type? ValueType);
        
