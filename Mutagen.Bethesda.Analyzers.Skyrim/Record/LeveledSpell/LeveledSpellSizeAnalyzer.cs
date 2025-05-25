using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.LeveledSpell;

public class LeveledSpellSizeAnalyzer : IIsolatedRecordAnalyzer<ILeveledSpellGetter>
{
    public static readonly TopicDefinition<int> TooManyEntries = MutagenTopicBuilder.FromDiscussion(
            364,
            "Too Many Entries in Leveled Spell",
            Severity.Error)
        .WithFormatting<int>("Leveled Spell has {0} which is more than the maximum of 255 entries");

    public IEnumerable<TopicDefinition> Topics { get; } = [TooManyEntries];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ILeveledSpellGetter> param)
    {
        var entries = param.Record.Entries;
        if (entries is null) return;

        if (entries.Count > 255)
        {
            param.AddTopic(
                TooManyEntries.Format(entries.Count));
        }
    }

    public IEnumerable<Func<ILeveledSpellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Entries;
    }
}
