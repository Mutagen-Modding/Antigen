using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Contextual;

public class UnownedBedAnalyzer : IContextualAnalyzer
{
    public static readonly TopicDefinition<IPlacedObjectGetter, ICellGetter> UnownedBed = MutagenTopicBuilder.FromDiscussion(
            209,
            "Unowned Bed in Owned Cell",
            Severity.Suggestion)
        .WithFormatting<IPlacedObjectGetter, ICellGetter>("Unowned bed placement {0} in owned cell {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [UnownedBed];

    public void Analyze(ContextualAnalyzerParams param)
    {
        foreach (var cell in param.LinkCache.PriorityOrder.WinningOverrides<ICellGetter>())
        {
            if (cell.IsExteriorCell()) continue;

            // If the cell is public or unowned, the bed can be unowned too
            if (cell.IsPublic() || cell.Owner.IsNull) continue;

            foreach (var placedObject in cell.GetAllPlaced(param.LinkCache).OfType<IPlacedObjectGetter>())
            {
                if (placedObject.IsDeleted) continue;

                // Owned beds are not a problem
                if (!placedObject.Owner.IsNull) continue;

                if (!param.LinkCache.TryResolve<IFurnitureGetter>(placedObject.Base.FormKey, out var furniture)) continue;
                if (!furniture.IsBed()) continue;

                var context = param.LinkCache.ResolveSimpleContext<IPlacedObjectGetter>(placedObject.FormKey);
                param.AddTopic(
                    context.ModKey,
                    placedObject,
                    UnownedBed.Format(placedObject, cell)
                );
            }
        }
    }
}
