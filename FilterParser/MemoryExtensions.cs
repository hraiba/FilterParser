namespace FilterParser;

public static class MemoryExtensions
{
    public static IEnumerable<ReadOnlyMemory<char>> Split(this ReadOnlySpan<char> source, char delimiter)
    {
        var segments = new List<ReadOnlyMemory<char>>();

        int startIndex = 0;
        int delimiterIndex;

        while ((delimiterIndex = source[startIndex..].IndexOf(delimiter)) != -1)
        {
            segments.Add(source.Slice(startIndex, delimiterIndex).ToArray());
            startIndex += delimiterIndex + 1;
        }

        segments.Add(source.Slice(startIndex).ToArray());

        return segments.ToArray();
    }
}

