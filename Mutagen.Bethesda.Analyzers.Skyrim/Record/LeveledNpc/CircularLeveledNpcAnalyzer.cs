using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.LeveledNpc;

public class CircularLeveledNpcListAnalyzer : IContextualRecordAnalyzer<ILeveledNpcGetter>
{
    public static readonly TopicDefinition CircularLeveledNpc = MutagenTopicBuilder.FromDiscussion(
            242,
            "Circular Leveled Npc",
            Severity.Suggestion)
        .WithoutFormatting("Leveled Npc contains itself");

    public IEnumerable<TopicDefinition> Topics { get; } = [CircularLeveledNpc];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ILeveledNpcGetter> param)
    {
        CircularLeveledListAnalyzerUtil.FindCircularList(param, l =>
        {
            if (l.Entries is not null)
            {
                return l.Entries
                    .Select(x => x.Data)
                    .WhereNotNull()
                    .Select(x => x.Reference.FormKey);
            }

            return [];
        }, CircularLeveledNpc);
    }

    IEnumerable<Func<ILeveledNpcGetter, object?>> IContextualRecordAnalyzer<ILeveledNpcGetter>.FieldsOfInterest()
    {
        yield return x => x.Entries;
    }
}
