using FilterParserLib;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FilterParser.Tests;

public class FilterParserQueryableTest
{
    [Fact]
    public void Given_Filter_With_And_Logic_Return_IQueryable()
    {
        //Arrange
        const string Filter = @"
{
  ""filters"": [
    {
      ""field"": ""Name"",
      ""operation"": ""contains"",
      ""value"": ""Mo""
    },
    {
      ""operation"": ""contains"",
      ""field"": ""LastName"",
      ""value"": ""Hr""
    },
    {
      ""operation"": ""equal"",
      ""field"": ""Mark"",
      ""value"": 9
    }
  ],
  ""operator"": ""and""
}";


        using var dbContext = GetDbContextWithEntity();
        var students = dbContext.Set<Student>().AsNoTracking();
        
        //Act
        var result = students.Filter(Filter).Select(x => x.Mark);
        var enumerateResult = result.ToList();
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IQueryable<int>>(result);
        Assert.NotEmpty(enumerateResult);
        Assert.Equal(1, result.Count());
    }

    private DbContext GetDbContextWithEntity()
    {
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: $"Test.{Random.Shared.Next()}")
            .Options;
        var context = new Context(options);
        var students = new List<Student>()
        {
            new() {Name = "Mohammad", LastName = "Hraiba", Mark = 9},
            new() {Name = "Louay", LastName = "Hraiba", Mark = 8},
        };
        context.Students.AddRange(students);
        context.SaveChanges();
        return context;
    }
}
