using FluentAssertions;
using Xunit;

namespace FilterParser.Tests;

public class FilterServiceTests
{
    [Fact]
    public void ShouldReturnEmptyGroups()
    {
        var service = new FilterService(new Tokenization());
        var result = service.Tokenize(typeof(FilterServiceTests), string.Empty);
        result.Should().NotBeNull();
        result.AndGroup.Should().BeEmpty();
        result.OrGroup.Should().BeEmpty();
    }
    
}