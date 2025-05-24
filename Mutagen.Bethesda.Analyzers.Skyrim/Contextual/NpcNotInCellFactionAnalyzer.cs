using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Contextual;

public class NpcNotInCellFactionAnalyzer : IContextualAnalyzer
{
    public static readonly TopicDefinition<INpcGetter, IFactionGetter> NpcNotInCellFaction = MutagenTopicBuilder.FromDiscussion(
            208,
            "Npc Not In Cell Faction",
            Severity.Suggestion)
        .WithFormatting<INpcGetter, IFactionGetter>("Npc {0} is not in their cell owner faction {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [NpcNotInCellFaction];

    public void Analyze(ContextualAnalyzerParams param)
    {
        foreach (var cell in param.LinkCache.PriorityOrder.WinningOverrides<ICellGetter>())
        {
            if (cell.Owner.IsNull) continue;
            if (!param.LinkCache.TryResolve<IFactionGetter>(cell.Owner.FormKey, out var cellOwnerFaction)) continue;

            foreach (var placedNpc in cell.GetAllPlaced(param.LinkCache).OfType<IPlacedNpcGetter>())
            {
                if (placedNpc.IsDeleted) continue;

                var npc = placedNpc.Base.TryResolve(param.LinkCache);
                if (npc is null || !npc.IsUnique()) continue;

                // Skip npcs with cell owner faction
                if (npc.Factions.Any(r => r.Faction.FormKey == cellOwnerFaction.FormKey)) continue;

                // Skip if cell is inn and npc is not innkeeper or server
                if (!param.LinkCache.TryResolve<ILocationGetter>(cell.Location.FormKey, out var location)) continue;
                if (location.IsInnLocation() && !npc.HasFaction(
                        param.LinkCache,
                        editorId => editorId is not null
                                    && (editorId.Contains("JobInn", StringComparison.Ordinal)
                                        || editorId.Contains("JobBard", StringComparison.Ordinal))))
                {
                    continue;
                }

                // Skip prisoners
                if (npc.HasFaction(param.LinkCache, editorId => editorId is not null && editorId.Contains("Prisoner"))) continue;

                var context = param.LinkCache.ResolveSimpleContext<INpcGetter>(npc.FormKey);
                param.AddTopic(
                    context.ModKey,
                    npc,
                    NpcNotInCellFaction.Format(npc, cellOwnerFaction));
            }
        }
    }
}
