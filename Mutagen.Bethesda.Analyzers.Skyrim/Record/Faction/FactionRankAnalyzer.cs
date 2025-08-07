using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Faction;

public class FactionRankAnalyzer : IIsolatedRecordAnalyzer<IFactionGetter>
{
    public static readonly TopicDefinition<int> NoRankName = MutagenTopicBuilder.FromDiscussion(
            407,
            "Faction Rank without Title",
            Severity.Suggestion)
        .WithFormatting<int>("Faction rank {0} has no title");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoRankName];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IFactionGetter> param)
    {
        var faction = param.Record;

        for (var i = 0; i < faction.Ranks.Count; i++)
        {
            var rank = faction.Ranks[i];
            if (rank.Title is null)
            {
                param.AddTopic(
                    NoRankName.Format(i));
            }
        }
    }

    public IEnumerable<Func<IFactionGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.VendorValues;
    }
}
