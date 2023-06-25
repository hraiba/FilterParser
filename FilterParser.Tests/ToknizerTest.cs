    [Fact]
    public void ShouldReturnListOfReadOnlyMemory()
    {
        const string filter = "(name=John|id=1);(age=20|isAdmin=true);(age=20|isAdmin=true)";
        ReadOnlySpan<char> span = filter.AsSpan();
        var expected = filter.Split(';');
        var result = span.Split(';').ToArray();
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().HaveCount(3);
        result[0].ToString().Should().Be(expected[0]);
        result[1].ToString().Should().Be(expected[1]);
        result[2].ToString().Should().Be(expected[2]);
    }
}
