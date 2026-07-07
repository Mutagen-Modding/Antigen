using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc.Unique;

public class NoItemsAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition NoItems = MutagenTopicBuilder.FromDiscussion(
            281,
            "Unique Npc Has Empty Inventory",
            Severity.Suggestion)
        .WithoutFormatting("Unique Npc has no items in inventory");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoItems];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (!npc.IsUniqueActorType(param.LinkCache)) return;

        // Skip NPCs using templates for inventory
        if (npc.Configuration.TemplateFlags.HasFlag(NpcConfiguration.TemplateFlag.Inventory)) return;

        if (npc.Items is null || npc.Items.Count == 0)
        {
            param.AddTopic(NoItems.Format());
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Configuration.TemplateFlags;
        yield return x => x.Keywords;
        yield return x => x.MajorFlags;
        yield return x => x.Items;
    }
}
