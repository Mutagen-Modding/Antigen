using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class RedundantKeywordAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition<IFormLinkGetter<IKeywordGetter>, IRaceGetter> RedundantKeyword = MutagenTopicBuilder.FromDiscussion(
            358,
            "Npc has Redundant Keyword",
            Severity.Suggestion)
        .WithFormatting<IFormLinkGetter<IKeywordGetter>, IRaceGetter>("Npc has redundant keyword {0} which is already on the npc's race {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [RedundantKeyword];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;

        if (npc.Keywords is null || npc.Keywords.Count == 0) return;

        var race = npc.Race.TryResolve(param.LinkCache);
        if (race?.Keywords is null || race.Keywords.Count == 0) return;

        var combinedKeywords = race.Keywords.Intersect(npc.Keywords).ToList();
        foreach (var keyword in combinedKeywords)
        {
            param.AddTopic(
                RedundantKeyword.Format(keyword, race));
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Keywords;
        yield return x => x.Race;
    }
}
