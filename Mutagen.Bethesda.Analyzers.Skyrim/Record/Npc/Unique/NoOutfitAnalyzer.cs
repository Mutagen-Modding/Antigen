using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc.Unique;

public class NoOutfitAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition NoOutfit = MutagenTopicBuilder.FromDiscussion(
            282,
            "No Outfit",
            Severity.Warning)
        .WithoutFormatting("Npc doesn't wear any outfit");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoOutfit];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (!npc.IsUniqueActorType(param.LinkCache)) return;

        // Skip NPCs using templates for inventory
        if (npc.Configuration.TemplateFlags.HasFlag(NpcConfiguration.TemplateFlag.Inventory)) return;

        if (npc.DefaultOutfit.IsNull)
        {
            param.AddTopic(NoOutfit.Format());
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Configuration.Flags;
        yield return x => x.Configuration.TemplateFlags;
        yield return x => x.Keywords;
        yield return x => x.DefaultOutfit;
    }
}
