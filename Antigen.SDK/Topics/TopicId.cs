namespace Antigen.SDK.Topics;

public record TopicId
{

    public TopicPrefix Prefix { get; }
    public ushort Id { get; }

    private readonly string _str;

    public TopicId(TopicPrefix prefix, ushort id)
    {
        Prefix = prefix;
        Id = id;
        _str = $"{Prefix}{Id}";
    }

    public override string ToString()
    {
        return _str;
    }

    public static implicit operator TopicId(string str)
    {
        return Parse(str);
    }

    public static TopicId Parse(ReadOnlySpan<char> str)
    {
        if (str.IsEmpty)
        {
            throw new ArgumentException("Input was empty", nameof(str));
        }

        ushort? id = null;
        int index = str.Length - 1;
        for (; index >= 0; index--)
        {
            if (char.IsNumber(str[index])) continue;
            if (index == str.Length - 1)
            {
                throw new ArgumentException($"Input was had no numeric end: {str}", nameof(str));
            }

            id = ushort.Parse(str[(index + 1)..]);
            break;
        }

        if (id is null)
        {
            throw new ArgumentException($"Input was not able to yield a numeric ID: {str}", nameof(str));
        }
        return new TopicId(new TopicPrefix(str[..(index + 1)].ToString()), id.Value);
    }
}
