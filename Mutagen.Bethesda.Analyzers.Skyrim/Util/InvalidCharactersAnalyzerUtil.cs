using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Util;

public static class InvalidCharactersAnalyzerUtil
{
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
