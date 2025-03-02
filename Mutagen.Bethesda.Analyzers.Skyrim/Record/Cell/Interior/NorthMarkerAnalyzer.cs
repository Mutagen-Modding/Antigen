using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Interior;

public class NorthMarkerAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition NoNorthMarker = MutagenTopicBuilder.DevelopmentTopic(
            "No North Marker",
            Severity.Suggestion)
        .WithoutFormatting("Missing north marker");

    public static readonly TopicDefinition<int> MoreThanOneNorthMarker = MutagenTopicBuilder.DevelopmentTopic(
            "More Than One North Marker",
            Severity.Suggestion)
        .WithFormatting<int>("Cell has {0} north markers when only one is permitted");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoNorthMarker, MoreThanOneNorthMarker];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;
        if (cell.IsExteriorCell()) return;

        var northMarkers = cell.GetAllPlaced(param.LinkCache)
            .OfType<IPlacedObjectGetter>()
            .Where(placed => placed.Base.FormKey == FormKeys.SkyrimSE.Skyrim.Static.NorthMarker.FormKey)
            .ToArray();

        var context = param.LinkCache.ResolveSimpleContext(cell);
        if (northMarkers.Length == 0)
        {
            param.AddTopic(
                context.ModKey,
                cell,
                NoNorthMarker.Format());
        }

        if (northMarkers.Length > 1)
        {
            param.AddTopic(
                context.ModKey,
                cell,
                MoreThanOneNorthMarker.Format(northMarkers.Length),
                ("NorthMarkers", northMarkers));
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Temporary;
        yield return x => x.Persistent;
    }
}
