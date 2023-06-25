namespace FilterParser;

public record FilterValue(ConjunctionToken Operation,  string Token, object? Value, Type? Type);