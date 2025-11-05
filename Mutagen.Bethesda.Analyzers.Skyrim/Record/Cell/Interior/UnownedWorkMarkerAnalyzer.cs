using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Interior;

public class UnownedWorkMarkerAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<IPlacedObjectGetter, ICellGetter> UnownedBed = MutagenTopicBuilder.FromDiscussion(
            210,
            "Unowned Work Marker in Owned Cell",
            Severity.Suggestion)
        .WithFormatting<IPlacedObjectGetter, ICellGetter>("Unowned work marker {0} in owned cell {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [UnownedBed];

    private static readonly HashSet<FormKey> WorkMarkers =
    [
        FormKeys.SkyrimSE.Skyrim.IdleMarker.SweepIdleMarker.FormKey,
        FormKeys.SkyrimSE.Skyrim.IdleMarker.IdleFarmingMarker.FormKey,
        FormKeys.SkyrimSE.Skyrim.Furniture.CounterBarLeanMarker.FormKey
    ];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;
        if (cell.IsExteriorCell()) return;
        if (cell.Owner.IsNull) return;

        foreach (var placedObject in cell.GetAllPlaced(param.LinkCache).OfType<IPlacedObjectGetter>())
        {
            if (placedObject.IsDeleted) continue;

            if (WorkMarkers.Contains(placedObject.Base.FormKey))
            {
                param.AddTopic(
                    UnownedBed.Format(placedObject, cell));
            }
        }

    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Owner;
        yield return x => x.Flags;
        yield return x => x.Temporary;
        yield return x => x.Persistent;
    }
}
