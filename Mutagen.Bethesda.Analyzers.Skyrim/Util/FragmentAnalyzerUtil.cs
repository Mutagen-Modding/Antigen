using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
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
            .GroupBy(nameSelector)
            .Where(g => g.CountGreaterThan(1));

        foreach (var dupe in duplicates)
        {
            param.AddTopic(topic.Format(dupe.Key), ("Usages", dupe));
        }
    }
}
