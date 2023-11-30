using FilterParserLib;
using Xunit;

namespace FilterParser.Tests;

public class FilterParserTests
{
    [Fact]
    public void Given_Filter_With_And_Logic_Return_IQueryable()
    {
        //Arrange
        const string filter = """
                     {
                       "filters": [
                         {
                           "field": "Name",
                           "keyword": "contains",
                           "value": "Mo"
                         },
                         {
                           "keyword": "contains",
                           "field": "LastName",
                           "value": "Hr"
                         },
                         {
                           "keyword": "equal",
                           "field": "Mark",
                           "value": 9
                         }
                       ],
                       "operator": "and"
                     }
                     """;
        var students = new List<Student>()
        {
            new() {Name = "Mohammad", LastName = "Hraiba", Mark = 9},
            new() {Name = "Louay", LastName = "Hraiba", Mark = 8},
        };
        
        //Act
        var result = FilterParser<Student>.ApplyFilter(students, filter);
        var enumerateResult = result.ToList();
        Assert.NotNull(result);
        Assert.NotEmpty(enumerateResult);
        Assert.Equal(1, result.Count());
    }

    [Fact]
    public void Given_Filter_With_Or_Logic_Return_IQueryable()
    {
        //Arrange

        const string filter = """
                              {
                                "filters": [
                                  {
                                    "field": "Name",
                                    "keyword": "contains",
                                    "value": "Mo"
                                  },
                                  {
                                    "keyword": "equal",
                                    "field": "Mark",
                                    "value": 8
                                  }
                                ],
                                "operator": "or"
                              }
                              """;
        var students = new List<Student>()
        {
            new() {Name = "Mohammad", LastName = "Hraiba", Mark = 9},
            new() {Name = "Louay", LastName = "Hraiba", Mark = 8},
            new() {Name = "Johndoe", LastName = "smith",  Mark = 1}
        };
        
        //Act
        var result = FilterParser<Student>.ApplyFilter(students, filter);
        var enumerateResult = result.ToList();
        Assert.NotNull(result);
        Assert.NotEmpty(enumerateResult);
        Assert.Equal(2, result.Count());
    }
}