namespace FilterParser.Tests;

public class Student
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public int Mark { get; set; }
}

public record Product(string ProductName, int Price, int UnitsInStock);