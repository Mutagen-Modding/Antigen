using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell;

public class NpcNotInCellFactionAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<INpcGetter, ICellGetter, IFactionGetter> NpcNotInCellFaction = MutagenTopicBuilder.FromDiscussion(
            208,
            "Npc Not In Cell Faction",
            Severity.Suggestion)
        .WithFormatting<INpcGetter, ICellGetter, IFactionGetter>("Npc {0} placed in {1} is not in their cell owner faction {2}");

    public IEnumerable<TopicDefinition> Topics { get; } = [NpcNotInCellFaction];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;
        if (cell.Owner.IsNull) return;

        if (!param.LinkCache.TryResolve<IFactionGetter>(cell.Owner.FormKey, out var cellOwnerFaction)) return;

        var isInn = param.LinkCache.TryResolve<ILocationGetter>(cell.Location.FormKey, out var location) && location.IsInnLocation();

        foreach (var placedNpc in cell.GetAllPlaced(param.LinkCache).OfType<IPlacedNpcGetter>())
        {
            if (placedNpc.IsDeleted) continue;

            var npc = placedNpc.Base.TryResolve(param.LinkCache);
            if (npc is null || !npc.IsUnique()) continue;

            // Skip npcs with cell owner faction
            if (npc.Factions.Any(r => r.Faction.FormKey == cellOwnerFaction.FormKey)) continue;

            // Skip if cell is inn and npc is not innkeeper or server
            if (isInn && !npc.HasFaction(
                    param.LinkCache,
                    editorId => editorId is not null
                                && (editorId.Contains("JobInn", StringComparison.Ordinal)
                                    || editorId.Contains("JobBard", StringComparison.Ordinal))))
            {
                continue;
            }

            // Skip prisoners
            if (npc.HasFaction(param.LinkCache, editorId => editorId is not null && editorId.Contains("Prisoner"))) continue;

            param.AddTopic(
                NpcNotInCellFaction.Format(npc, cell, cellOwnerFaction));
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Owner;
        yield return x => x.Location;
        yield return x => x.Temporary;
        yield return x => x.Persistent;
    }
}
