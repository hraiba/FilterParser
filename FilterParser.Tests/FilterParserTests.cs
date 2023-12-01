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
                                    "operation": "contains",
                                    "value": "Mo"
                                  },
                                  {
                                    "operation": "contains",
                                    "field": "LastName",
                                    "value": "Hr"
                                  },
                                  {
                                    "operation": "equal",
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
        var result = FilterParserLib.FilterParser.ApplyFilter(students, filter);
        var enumerateResult = result.ToList();
        
        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IQueryable>(result);
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
                                    "operation": "contains",
                                    "value": "Mo"
                                  },
                                  {
                                    "operation": "equal",
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
            new() {Name = "Johndoe", LastName = "smith", Mark = 1}
        };

        //Act
        var result = FilterParserLib.FilterParser.ApplyFilter(students, filter);
        var enumerateResult = result.ToList();
        
        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IQueryable>(result);
        Assert.NotEmpty(enumerateResult);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public void Given_Empty_Filter_Return_IQueryable()
    {
        //Arrange
        const string filter = """
                              {
                                "filters": [],
                                "operator": "and"
                              }
                              """;
        var students = new List<Student>()
        {
            new() {Name = "Mohammad", LastName = "Hraiba", Mark = 9},
            new() {Name = "Louay", LastName = "Hraiba", Mark = 8},
            new() {Name = "Johndoe", LastName = "smith", Mark = 1}
        };

        //Act
        var result = FilterParserLib.FilterParser.ApplyFilter(students, filter);
        var enumerateResult = result.ToList();
        
        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IQueryable>(result);
        Assert.NotEmpty(enumerateResult);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public void Given_Null_Filter_Return_IQueryable()
    {
        //Arrange
        const string? filter = null;
        var students = new List<Student>()
        {
            new() {Name = "Mohammad", LastName = "Hraiba", Mark = 9},
            new() {Name = "Louay", LastName = "Hraiba", Mark = 8},
            new() {Name = "Johndoe", LastName = "smith", Mark = 1}
        };

        //Act
        var result = FilterParserLib.FilterParser.ApplyFilter(students, filter);
        var enumerateResult = result.ToList();
        
        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IQueryable>(result);
        Assert.NotEmpty(enumerateResult);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public void Given_Null_Filter_And_Null_DataSet_Throw_NullArgumentException()
    {
        //Arrange
        const string? filter = null;
        IEnumerable<Student>? students = null;

        //Act && Assert
        Assert.Throws<ArgumentNullException>(() => FilterParserLib.FilterParser.ApplyFilter(students, filter));
    }

    [Fact]
    public void Given_Filter_With_And_Or_Operators_Return_IQueryable()
    {
        //Arrange
        const string filter = """
                              {
                                "filters": [
                                  {
                                    "filters":[
                                    {
                                    "field": "LastName",
                                    "operation": "contains",
                                    "value": "Hr"
                                    },
                                      {
                                        "field": "Name",
                                        "operation": "equal",
                                        "value": "Mohammad"
                                      }
                                    ],
                                    "operator": "and"
                                  },
                                  {
                                    "operation": "equal",
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
            new() {Name = "Louay", LastName = "Mo", Mark = 8},
            new() {Name = "Johndoe", LastName = "smith", Mark = 1}
        };

        //Act
        var result = FilterParserLib.FilterParser.ApplyFilter(students, filter);
        var enumerateResult = result.ToList();
        
        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IQueryable>(result);
        Assert.NotEmpty(enumerateResult);
        Assert.Equal(2, result.Count());
    }
    
    [Fact]
    public void Given_Filter_With_Or_And_Operators_Return_IQueryable()
    {
        //Arrange
        const string filter = """
                              {
                                "filters": [
                                  {
                                    "filters":[
                                    {
                                    "field": "LastName",
                                    "operation": "equal",
                                    "value": "smith"
                                    },
                                      {
                                        "field": "Name",
                                        "operation": "equal",
                                        "value": "Mohammad"
                                      }
                                    ],
                                    "operator": "or"
                                  },
                                  {
                                    "operation": "equal",
                                    "field": "Mark",
                                    "value": 8
                                  }
                                ],
                                "operator": "and"
                              }
                              """;
        var students = new List<Student>()
        {
            new() {Name = "Mohammad", LastName = "Hraiba", Mark = 9},
            new() {Name = "Louay", LastName = "Mo", Mark = 8},
            new() {Name = "Johndoe", LastName = "smith", Mark = 1}
        };

        //Act
        var result = FilterParserLib.FilterParser.ApplyFilter(students, filter);
        var enumerateResult = result.ToList();
        
        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IQueryable>(result);
        Assert.Empty(enumerateResult);
        Assert.Equal(0, result.Count());
    }
}