using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Interior;

public class ConflictingVoiceTypesAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<ICellGetter, int, IFormLinkGetter<IVoiceTypeGetter>> NpcsWithSameVoiceType = MutagenTopicBuilder.FromDiscussion(
            260,
            "NPCs with the same voice type in same cell",
            Severity.Suggestion)
        .WithFormatting<ICellGetter, int, IFormLinkGetter<IVoiceTypeGetter>>("Cell {0} includes {1} npcs with the same voice type {2}");

    public IEnumerable<TopicDefinition> Topics { get; } = [NpcsWithSameVoiceType];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;
        if (cell.IsExteriorCell()) return;

        var npcVoiceTypes = new Dictionary<IFormLinkGetter<INpcGetter>, IFormLinkGetter<IVoiceTypeGetter>>();
        foreach (var placedNpc in cell.GetAllPlaced(param.LinkCache).OfType<IPlacedNpcGetter>())
        {
            if (placedNpc.IsDeleted) continue;
            if (!param.LinkCache.TryResolve<INpcGetter>(placedNpc.Base.FormKey, out var npc)) continue;
            if (!npc.IsUnique()) continue;
            if (npc.Voice.IsNull) continue;

            npcVoiceTypes.TryAdd(npc.ToLink(), npc.Voice);
        }

        foreach (var grouping in npcVoiceTypes.GroupBy(x => x.Value))
        {
            var npcs = grouping.Select(x => x.Key).ToList();
            var count = npcs.Count;
            if (count <= 1) continue;

            var cellContext = param.LinkCache.ResolveSimpleContext(cell);
            param.AddTopic(
                cellContext.ModKey,
                cell,
                NpcsWithSameVoiceType.Format(cell, count, grouping.Key),
                ("NPCs", npcs));
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Temporary;
        yield return x => x.Persistent;
    }
}
