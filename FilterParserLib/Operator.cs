
namespace FilterParserLib;

public enum Operator
{
    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,
    Contains,
    StartWith,
}

public enum Logic
{
    And,
    Or,
}