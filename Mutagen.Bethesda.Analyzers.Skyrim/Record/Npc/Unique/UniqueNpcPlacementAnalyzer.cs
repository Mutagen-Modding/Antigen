using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc.Unique;

public class UniqueNpcPlacementAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition PlacedNever = MutagenTopicBuilder.FromDiscussion(
            484,
            "Unique NPC Never Placed",
            Severity.Suggestion)
        .WithoutFormatting("Unique NPC is never placed in the world");

    public static readonly TopicDefinition PlacedMultiple = MutagenTopicBuilder.FromDiscussion(
            485,
            "Unique NPC Placed Multiple Times",
            Severity.Warning)
        .WithoutFormatting("Unique NPC is placed multiple times in the world");

    public IEnumerable<TopicDefinition> Topics { get; } = [PlacedNever, PlacedMultiple];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (!npc.IsUnique()) return;

        var placements = param.ResolveCache<ILinkUsageCache>()
            .GetUsagesOf<IPlacedNpcGetter>(npc).UsageLinks
            .Select(p => p.Resolve(param.LinkCache))
            .Where(p => p.Base.Equals(npc))
            .ToArray();

        switch (placements.Length)
        {
            case 0:
                param.AddTopic(
                    PlacedNever.Format());
                break;
            case > 1:
                var notDeadNpcs = placements
                    .Where(p => !p.MajorFlags.HasFlag(PlacedNpc.MajorFlag.StartsDead))
                    .ToArray();

                if (notDeadNpcs.Length > 1)
                {
                    // There are more than one placement where the Npc is not dead from the start
                    param.AddTopic(
                        PlacedMultiple.Format(),
                        ("Placements", notDeadNpcs));
                }

                break;
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Configuration.Flags;
        yield return x => x.Configuration.TemplateFlags;
        yield return x => x.Keywords;
        yield return x => x.Voice;
    }
}
