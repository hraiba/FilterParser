using FluentAssertions;
using Xunit;

namespace FilterParser.Tests;

public class TokenizerTest
{
    [Fact]
    public void ShouldReturnGroup()
    {
        var filter = "(name=John|id=1);(age=20|isAdmin=true);(name=20|isAdmin=true);";
        ITokenization tokenization = new Tokenization();
        var result = tokenization.Tokenize(typeof(Model), filter);
        result.Should().NotBeNull();
    }
}

public class Model
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public bool IsAdmin { get; set; }
}