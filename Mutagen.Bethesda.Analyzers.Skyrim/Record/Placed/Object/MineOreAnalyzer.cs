using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Object;

public class MineOreAnalyzer : IContextualRecordAnalyzer<IPlacedObjectGetter>
{
    public static readonly TopicDefinition NoFurnitureLinked = MutagenTopicBuilder.FromDiscussion(
            287,
            "Mine Ore without Linked Furniture",
            Severity.Error)
        .WithoutFormatting("Mine Ore has no furniture linked");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoFurnitureLinked];

    private static readonly HashSet<IFormLinkGetter<IPlaceableObjectGetter>> MiningFurniture =
    [
        FormKeys.SkyrimSE.Skyrim.Furniture.PickaxeMiningTableMarker,
        FormKeys.SkyrimSE.Skyrim.Furniture.PickaxeMiningFloorMarker,
        FormKeys.SkyrimSE.Skyrim.Furniture.PickaxeMiningWallMarker,
        FormKeys.SkyrimSE.Skyrim.Furniture.PickaxeMiningTableMarkerNonPlayer
    ];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedObjectGetter> param)
    {
        var placedObject = param.Record;

        // Skip objects that don't place mine activators
        if (!param.LinkCache.TryResolve<IActivatorGetter>(placedObject.Base.FormKey, out var mine)) return;
        if (mine.EditorID is null || (!mine.EditorID.Contains("MineOre") && !mine.EditorID.Contains("MineGem"))) return;

        foreach (var linkedRef in placedObject.LinkedReferences)
        {
            if (!param.LinkCache.TryResolve<IPlacedObjectGetter>(linkedRef.Reference.FormKey, out var linkedObject)) continue;

            if (MiningFurniture.Contains(linkedObject.Base)) return;
        }

        param.AddTopic(
            NoFurnitureLinked.Format());
    }

    public IEnumerable<Func<IPlacedObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.EditorID;
        yield return x => x.Base;
        yield return x => x.LinkedReferences;
    }
}
