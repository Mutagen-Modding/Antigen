using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc.Unique;

public class NoCrimeFactionAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition NoCrimeFaction = MutagenTopicBuilder.FromDiscussion(
            280,
            "No Crime Faction",
            Severity.Warning)
        .WithoutFormatting("Npc has no crime faction");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoCrimeFaction];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (!npc.IsEligibleForTest(param.LinkCache)) return;

        // Skip NPCs using templates for factions
        if (npc.Configuration.TemplateFlags.HasFlag(NpcConfiguration.TemplateFlag.Factions)) return;

        // Skip NPCs who don't care about crime
        if (npc.AIData.Responsibility == Responsibility.NoCrime) return;

        if (npc.CrimeFaction.IsNull)
        {
            param.AddTopic(
                NoCrimeFaction.Format());
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Configuration.TemplateFlags;
        yield return x => x.Configuration.Flags;
        yield return x => x.AIData.Responsibility;
        yield return x => x.CrimeFaction;
        yield return x => x.Keywords;
    }
}
