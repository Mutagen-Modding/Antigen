using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;

namespace Antigen.Skyrim.Util;

public static class InvalidCharactersAnalyzerUtil
{
    public static Dictionary<char, string> InvalidStrings { get; } = new()
    {
        { '’', "'" },
        { '`', "'" },
        { '”', "\"" },
        { '“', "\"" },
        { '…', "..." },
        { '—', "-" },
    };

    public static char[] InconsistentStrings { get; } = ['[', ']'];

    public static void CheckInconsistentCharacters<T>(IsolatedRecordAnalyzerParams<T> param, string text, Language language, TopicDefinition<string, Language> topic) where T : IMajorRecordGetter
    {
        var foundCharacters = InconsistentStrings.Where(text.Contains).ToArray();
        if (foundCharacters.Length == 0) return;

        param.AddTopic(
            topic.Format(text, language),
            ("Inconsistent Characters", foundCharacters));
    }
}
