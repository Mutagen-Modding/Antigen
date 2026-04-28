using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc.Unique;

public class MultipleBedOwnershipAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition<int> NpcOwnsMultipleBeds = MutagenTopicBuilder.FromDiscussion(
            493,
            "Unique Npc Owns Multiple Beds",
            Severity.Suggestion)
        .WithFormatting<int>("Unique Npc owns {0} beds");

    public static readonly TopicDefinition NpcOwnsNoBeds = MutagenTopicBuilder.FromDiscussion(
            494,
            "Unique Npc Owns No Beds",
            Severity.Warning)
        .WithoutFormatting("Unique Npc does not own any beds");

    public IEnumerable<TopicDefinition> Topics { get; } = [NpcOwnsMultipleBeds, NpcOwnsNoBeds];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (!npc.IsUniqueActorType(param.LinkCache)) return;

        var usageCache = param.ResolveCache<ILinkUsageCache>();
        var ownedBeds = usageCache.GetUsagesOf<IPlacedObjectGetter>(npc).UsageLinks
            .Where(x => IsOwner(x, npc.FormKey))
            .ToList();

        if (ownedBeds.Count == 0)
        {
            var factionOwnedBeds = npc.Factions.SelectMany(rank => usageCache.GetUsagesOf<IPlacedObjectGetter>(rank.Faction).UsageLinks
                    .Where(x => IsOwner(x, rank.Faction.FormKey)))
                .ToList();

            if (factionOwnedBeds.Count == 0)
            {
                param.AddTopic(
                    NpcOwnsNoBeds.Format());
            }
        }
        else if (ownedBeds.Count > 1)
        {
            param.AddTopic(
                NpcOwnsMultipleBeds.Format(ownedBeds.Count),
                ("Owned Beds", ownedBeds));
        }

        bool IsOwner(IFormLinkGetter<IMajorRecordGetter> usageLink, FormKey owner)
        {
            return usageLink.TryResolve<IPlacedObjectGetter>(param.LinkCache, out var placedObject)
                   && placedObject.Owner.FormKey == owner
                   && placedObject.Base.TryResolve<IFurnitureGetter>(param.LinkCache, out var furniture)
                   && furniture.IsBed();
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield break;
    }
}
