using System.Text.RegularExpressions;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Book;

public partial class FontAnalyzer : IIsolatedRecordAnalyzer<IBookGetter>
{
    // Regex is far from ideal to parse HTML-like tags, but will do here.
    [GeneratedRegex(@"face=([""'$a-z]+)[ >]", RegexOptions.IgnoreCase)]
    private static partial Regex FontTagRegex();

    public static readonly TopicDefinition<string, Language> InvalidFont = MutagenTopicBuilder.FromDiscussion(
            647,
            "Invalid font",
            Severity.Error)
        .WithFormatting<string, Language>("Book references invalid font {0} in language {1}");

    // TODO: Parse fontconfig.txt
    public static readonly HashSet<string> ValidFonts = new(StringComparer.OrdinalIgnoreCase) {
        "$SkyrimBooks",
        "$HandwrittenFont",
        "$HandwrittenBold",
        "$DaedricFont",
        "$DragonFont",
        "$DwemerFont",
        "$FalmerFont",
        "$MageScriptFont"
    };

    public IEnumerable<TopicDefinition> Topics => [InvalidFont];

    static bool FontTagValid(string font)
    {
        if (!font.StartsWith('\'') && !font.StartsWith('"'))
            return false;
        if (!font.EndsWith('\'') && !font.EndsWith('"'))
            return false;
        var unquoted = font[1..^1];
        return ValidFonts.Contains(unquoted);
    }

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IBookGetter> param)
    {
        var book = param.Record;

        foreach (var (language, text) in book.BookText)
        {
            var matches = FontTagRegex().Matches(text);
            foreach (Match match in matches)
            {
                var font = match.Groups[1];
                if (!FontTagValid(font.Value))
                    param.AddTopic(InvalidFont.Format(font.Value, language));
            }
        }
    }

    public IEnumerable<Func<IBookGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.BookText;
    }
}
