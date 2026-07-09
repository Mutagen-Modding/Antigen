using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc.Unique;

public class NoCombatStyleAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition NoCombatStyleFaction = MutagenTopicBuilder.FromDiscussion(
            404,
            "Unique Npc Has No Combat Style",
            Severity.None)
        .WithoutFormatting("Unique Npc has no combat style");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoCombatStyleFaction];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (!npc.IsUniqueActorType(param.LinkCache)) return;

        if (npc.CombatStyle.IsNull)
        {
            param.AddTopic(
                NoCombatStyleFaction.Format());
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.CombatStyle;
        yield return x => x.Name;
    }
}
