using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Antigen.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Antigen.Skyrim.Record.LeveledItem;

public class CircularLeveledItemListAnalyzer : IContextualRecordAnalyzer<ILeveledItemGetter>
{
    public static readonly TopicDefinition CircularLeveledItem = MutagenTopicBuilder.FromDiscussion(
            230,
            "Circular Leveled Item",
            Severity.CTD)
        .WithoutFormatting("Leveled Item contains itself in path {0}");

    public IEnumerable<TopicDefinition> Topics { get; } = [CircularLeveledItem];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ILeveledItemGetter> param)
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
        }, CircularLeveledItem);
    }

    IEnumerable<Func<ILeveledItemGetter, object?>> IContextualRecordAnalyzer<ILeveledItemGetter>.FieldsOfInterest()
    {
        yield return x => x.Entries;
    }
}
