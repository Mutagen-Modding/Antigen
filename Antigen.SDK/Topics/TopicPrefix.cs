using Noggog;

namespace Antigen.SDK.Topics;

public record TopicPrefix
{
    public const byte MaxPrefixSize = 4;

    public string String { get; }

    public TopicPrefix(string prefix)
    {
        if (prefix.IsNullOrEmpty())
        {
            throw new ArgumentException("Prefix must have content", nameof(prefix));
        }

        if (prefix.Length > MaxPrefixSize)
        {
            throw new ArgumentException($"Prefix is above the maximum of {MaxPrefixSize}: {prefix}", nameof(prefix));
        }

        foreach (var c in prefix)
        {
            if (char.IsNumber(c))
            {
                throw new ArgumentException($"Prefix cannot have numbers: {prefix}", nameof(prefix));
            }

            if (c == '.')
            {
                throw new ArgumentException($"Prefix has invalid char '.': {prefix}", nameof(prefix));
            }
        }

        String = prefix;
    }

    public static implicit operator string(TopicPrefix prefix) => prefix.String;

    public static implicit operator TopicPrefix(string str) => new(str);

    public override string ToString() => String;
}
