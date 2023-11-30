using Microsoft.EntityFrameworkCore;


namespace FilterParser.Tests;

public class Context : DbContext
{
    public DbSet<Student> Students { get; set; }

    public Context(DbContextOptions<Context> options) : base(options)
    {
    }

    public void AddData()
    {
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: "Test")
            .Options;
        using var context = new Context(options);
        var students = new List<Student>()
        {
            new() {Name = "Mohammad", LastName = "Hraiba", Mark = 9},
            new() {Name = "Louay", LastName = "Hraiba", Mark = 8},
        };
        Students.AddRange(students);
        SaveChanges();
    }
}