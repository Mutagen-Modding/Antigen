using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Interior;

public class UnownedBedAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<IPlacedObjectGetter, ICellGetter> UnownedBed = MutagenTopicBuilder.FromDiscussion(
            209,
            "Unowned Bed in Owned Cell",
            Severity.Suggestion)
        .WithFormatting<IPlacedObjectGetter, ICellGetter>("Unowned bed placement {0} in owned cell {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [UnownedBed];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;
        if (cell.IsExteriorCell()) return;

        // If the cell is public or unowned, the bed can be unowned too
        if (cell.IsPublic() || cell.Owner.IsNull) return;

        foreach (var placedObject in cell.GetAllPlaced(param.LinkCache).OfType<IPlacedObjectGetter>())
        {
            if (placedObject.IsDeleted) continue;

            // Owned beds are not a problem
            if (!placedObject.Owner.IsNull) continue;

            if (!param.LinkCache.TryResolve<IFurnitureGetter>(placedObject.Base.FormKey, out var furniture)) continue;
            if (!furniture.IsBed()) continue;

            var context = param.LinkCache.ResolveSimpleContext<IPlacedObjectGetter>(placedObject.FormKey);
            param.AddTopic(
                UnownedBed.Format(placedObject, cell)
            );
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.Owner;
        yield return x => x.Temporary;
        yield return x => x.Persistent;
    }
}
