using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.LeveledItem;

public class LeveledItemSizeAnalyzer : IIsolatedRecordAnalyzer<ILeveledItemGetter>
{
    public static readonly TopicDefinition<int> TooManyEntries = MutagenTopicBuilder.FromDiscussion(
            362,
            "Too Many Entries in Leveled Item",
            Severity.Error)
        .WithFormatting<int>("Leveled Item has {0} which is more than the maximum of 255 entries");

    public IEnumerable<TopicDefinition> Topics { get; } = [TooManyEntries];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ILeveledItemGetter> param)
    {
        var entries = param.Record.Entries;
        if (entries is null) return;

        if (entries.Count > 255)
        {
            param.AddTopic(
                TooManyEntries.Format(entries.Count));
        }
    }

    public IEnumerable<Func<ILeveledItemGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Entries;
    }
}
