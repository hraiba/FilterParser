namespace FilterParser;

public enum FilterToken
{
  
    Unknown,
    Active,
    Upcoming,
    OfferType,
    Partner,
    StartDate,
    EndDate,
    SearchText,
}

public enum OperatorToken
{
    Equal,
    GreaterThan,
    LessThan,
    Contains,
    StartsWith,
    EndsWith,
}

public enum ConjunctionToken
{
    And,
    Or,
}
