using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Contextual;

public class UnownedWorkMarkerAnalyzer : IContextualAnalyzer
{
    public static readonly TopicDefinition<IPlacedObjectGetter, ICellGetter> UnownedBed = MutagenTopicBuilder.DevelopmentTopic(
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

    public void Analyze(ContextualAnalyzerParams param)
    {
        foreach (var cell in param.LinkCache.PriorityOrder.WinningOverrides<ICellGetter>())
        {
            if (cell.IsExteriorCell()) continue;
            if (cell.Owner.IsNull) continue;

            foreach (var placedObject in cell.GetAllPlaced(param.LinkCache).OfType<IPlacedObjectGetter>())
            {
                if (WorkMarkers.Contains(placedObject.Base.FormKey))
                {
                    var context = param.LinkCache.ResolveSimpleContext<IPlacedObjectGetter>(placedObject.FormKey);
                    param.AddTopic(
                        context.ModKey,
                        placedObject,
                        UnownedBed.Format(placedObject, cell));
                }
            }
        }
    }
}
