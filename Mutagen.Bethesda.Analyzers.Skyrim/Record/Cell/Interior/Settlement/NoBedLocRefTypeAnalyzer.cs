using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Interior.Settlement;

public class NoBedLocRefTypeAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<IPlacedObjectGetter> NoBedLocRefType = MutagenTopicBuilder.DevelopmentTopic(
            "No Bed Location Reference Type",
            Severity.Suggestion)
        .WithFormatting<IPlacedObjectGetter>("{0} is a bed and should have HouseBedRefType location reference type");

    public static readonly TopicDefinition<IPlacedObjectGetter> InvalidBedLocRefType = MutagenTopicBuilder.DevelopmentTopic(
            "Invalid Bed Location Reference Type",
            Severity.Error)
        .WithFormatting<IPlacedObjectGetter>("{0} is not a bed and should not have HouseBedRefType location reference type");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoBedLocRefType, InvalidBedLocRefType];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;

        // Skip non-settlement cells
        if (!cell.IsSettlementCell(param.LinkCache)) return;

        foreach (var placedObject in cell.GetAllPlaced(param.LinkCache).OfType<IPlacedObjectGetter>())
        {
            if (placedObject.IsDeleted) continue;

            var isBed = placedObject.IsBed(param.LinkCache);
            var hasLocRefType = placedObject.HasLocationRefType(FormKeys.SkyrimSE.Skyrim.LocationReferenceType.HouseBedRefType);

            if (isBed && !hasLocRefType)
            {
                param.AddTopic(
                    NoBedLocRefType.Format(placedObject));
            }
            else if (!isBed && hasLocRefType)
            {
                param.AddTopic(
                    InvalidBedLocRefType.Format(placedObject));
            }
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.Location;
        yield return x => x.Temporary;
    }
}
