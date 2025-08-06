using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class GuardWithoutCrimeFactionAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition InvalidInterruptOverridePackageUsage = MutagenTopicBuilder.FromDiscussion(
            406,
            "Guard without Crime Faction",
            Severity.Warning)
        .WithoutFormatting("Npc has IsGuardFaction but has no crime faction assigned");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidInterruptOverridePackageUsage];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (!npc.CrimeFaction.IsNull) return;

        if (npc.Factions.Any(x => x.Faction.FormKey == FormKeys.SkyrimSE.Skyrim.Faction.IsGuardFaction.FormKey))
        {
            param.AddTopic(
                InvalidInterruptOverridePackageUsage.Format());
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Factions;
    }
}
