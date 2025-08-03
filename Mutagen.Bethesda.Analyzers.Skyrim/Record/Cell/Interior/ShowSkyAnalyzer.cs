using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;


namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Interior;

public class ShowSkyAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<IFormLinkNullableGetter<IRegionGetter>, ICellGetter, IPlacedObjectGetter> WrongRegion = MutagenTopicBuilder.FromDiscussion(
            391,
            "Weather/Sky Region Mismatch",
            Severity.Warning)
        .WithFormatting<IFormLinkNullableGetter<IRegionGetter>, ICellGetter, IPlacedObjectGetter>("The cell has sky enabled but its sky/weather from region {0} does not match the region of the cell {1} that the door {2} leads to");

    public static readonly TopicDefinition ShowSkyWithoutRegion = MutagenTopicBuilder.FromDiscussion(
            394,
            "ShowSky with no region",
            Severity.Warning)
        .WithoutFormatting("Cell has ShowSky flag but no sky/weather from region assigned");

    IEnumerable<TopicDefinition> IAnalyzer.Topics => [WrongRegion, ShowSkyWithoutRegion];

    void IContextualRecordAnalyzer<ICellGetter>.AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;

        if (cell.IsExteriorCell()) return;

        if (!cell.Flags.HasFlag(Bethesda.Skyrim.Cell.Flag.ShowSky)) return;

        var cellSkyAndWeatherFromRegion = cell.SkyAndWeatherFromRegion;
        if (cellSkyAndWeatherFromRegion.IsNull)
        {
            param.AddTopic(ShowSkyWithoutRegion.Format());
        }

        foreach (var exteriorDoor in cell.GetExteriorDoorsGoingIntoInteriorRecursively(param.LinkCache)) {
            var exteriorCell = exteriorDoor.GetCell(param.LinkCache);
            if (exteriorCell?.Regions is null) continue;

            if (!exteriorCell.Regions.Contains(cellSkyAndWeatherFromRegion))
            {
                param.AddTopic(
                    WrongRegion.Format(cellSkyAndWeatherFromRegion, exteriorCell, exteriorDoor));
            }
        }
    }

    IEnumerable<Func<ICellGetter, object?>> IContextualRecordAnalyzer<ICellGetter>.FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.SkyAndWeatherFromRegion;
        yield return x => x.Temporary;
        yield return x => x.Persistent;
    }
}

