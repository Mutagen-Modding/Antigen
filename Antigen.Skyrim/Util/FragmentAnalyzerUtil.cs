using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;

namespace Antigen.Skyrim.Util;
public static class FragmentAnalyzerUtil
{
    public static void CheckDuplicateFragments<T>(
        IsolatedRecordAnalyzerParams<T> param,
        TopicDefinition<string> topic,
        IEnumerable<string?> fragmentNames) where T : IMajorRecordGetter
    {
        var duplicates = fragmentNames
            .WhereNotNull()
            .GroupBy(f => f, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.CountGreaterThan(1));

        foreach (var dupe in duplicates)
        {
            param.AddTopic(topic.Format(dupe.Key));
        }
    }
}
