using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Util;
public static class FragmentAnalyzerUtil
{
    public static void CheckDuplicateFragments<T, U>(
        IsolatedRecordAnalyzerParams<T> param,
        TopicDefinition<string> topic,
        IEnumerable<U?> fragments,
        Func<U, string> nameSelector) where T : IMajorRecordGetter where U : class
    {
        var duplicates = fragments
            .WhereNotNull()
            .GroupBy(nameSelector, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.CountGreaterThan(1));

        foreach (var dupe in duplicates)
        {
            param.AddTopic(topic.Format(dupe.Key), ("Usages", dupe));
        }
    }
}
