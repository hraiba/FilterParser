namespace FilterParser;

public static class MemoryExtensions
{
    public static ReadOnlyMemory<ReadOnlyMemory<char>> Split(this ReadOnlySpan<char> source, char delimiter)
    {
        if (source.Length == 0 || source.IsWhiteSpace())
        {
            return Array.Empty<ReadOnlyMemory<char>>();
        }

        if (delimiter == '\0')
        {
            return new ReadOnlyMemory<char>[] { source.ToArray() };
        }
        
        var segments = new List<ReadOnlyMemory<char>>();

        var startIndex = 0;
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

