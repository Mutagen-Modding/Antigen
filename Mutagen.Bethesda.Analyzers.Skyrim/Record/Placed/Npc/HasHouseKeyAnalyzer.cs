using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Npc;

public class HasHouseKeyAnalyzer : IContextualRecordAnalyzer<IPlacedNpcGetter>
{
    public static readonly TopicDefinition<INpcGetter, ICellGetter, IPlacedObjectGetter, IKeyGetter> MissingHouseKey = MutagenTopicBuilder.FromDiscussion(
            472,
            "Npc Missing House Key",
            Severity.Warning)
        .WithFormatting<INpcGetter, ICellGetter, IPlacedObjectGetter, IKeyGetter>("Places Npc {0} in cell {1} from where the door {2} locked with key {3} leads to an exterior, but the Npc does not have the key in their inventory");

    public IEnumerable<TopicDefinition> Topics { get; } = [MissingHouseKey];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedNpcGetter> param)
    {
        var placedNpc = param.Record;
        if (placedNpc.MajorFlags.HasFlag(PlacedNpc.MajorFlag.StartsDead)) return;

        var npc = placedNpc.Base.TryResolve(param.LinkCache);
        if (npc is null) return;
        if (!npc.IsUnique()) return;
        if (!npc.IsActorTypeNpc(param.LinkCache)) return;

        var cell = placedNpc.GetCell(param.LinkCache);
        if (cell is null) return;

        var missingKeys = new HashSet<FormKey>();
        foreach (var exteriorDoor in cell.GetExteriorDoorsGoingIntoInteriorRecursively(param.LinkCache))
        {
            var interiorDoor = exteriorDoor.TeleportDestination?.Door.TryResolve(param.LinkCache);
            if (interiorDoor is null) continue;

            var key = interiorDoor.Lock?.Key.TryResolve(param.LinkCache);
            if (key is null) continue;

            if (npc.Items is not null && npc.Items.Any(entry => entry.Item.Item.FormKey == key.FormKey)) continue;

            if (missingKeys.Add(key.FormKey))
            {
                param.AddTopic(
                    MissingHouseKey.Format(npc, cell, interiorDoor, key));
            }
        }
    }

    public IEnumerable<Func<IPlacedNpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Base;
        yield return x => x.MajorFlags;
    }
}
