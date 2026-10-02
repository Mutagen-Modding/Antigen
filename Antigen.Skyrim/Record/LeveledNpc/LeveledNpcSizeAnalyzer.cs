using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.LeveledNpc;

public class LeveledNpcSizeAnalyzer : IIsolatedRecordAnalyzer<ILeveledNpcGetter>
{
    public static readonly TopicDefinition<int> TooManyEntries = MutagenTopicBuilder.FromDiscussion(
            363,
            "Too Many Entries in Leveled Npc",
            Severity.Error)
        .WithFormatting<int>("Leveled Npc has {0} which is more than the maximum of 255 entries");

    public IEnumerable<TopicDefinition> Topics { get; } = [TooManyEntries];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ILeveledNpcGetter> param)
    {
        var entries = param.Record.Entries;
        if (entries is null) return;

        if (entries.Count > 255)
        {
            param.AddTopic(
                TooManyEntries.Format(entries.Count));
        }
    }

    public IEnumerable<Func<ILeveledNpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Entries;
    }
}
