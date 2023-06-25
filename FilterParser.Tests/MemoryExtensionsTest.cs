using System;
using FluentAssertions;
using Xunit;

namespace FilterParser.Tests;

public class MemoryExtensionsTest
{
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

    [Fact]
    public void ShouldReturnEmptyListWhenFilterEmptyString()
    {
        const string filter = "";
        ReadOnlySpan<char> span = filter.AsSpan();
        var result = span.Split(';').ToArray();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldReturnEmptyListWhenFilterNull()
    {
        const string? filter = null;
        ReadOnlySpan<char> span = filter.AsSpan();
        var result = span.Split(';').ToArray();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldReturnSourceWhenDelimiterIsZero()
    {
        const string filter = "(name=John|id=1);(age=20|isAdmin=true);(age=20|isAdmin=true)";
        ReadOnlySpan<char> span = filter.AsSpan();
        var expected = filter.Split('\0');
        var result = span.Split('\0').ToArray();
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result[0].ToString().Should().Be(expected[0]);
    }
}