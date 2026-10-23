using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Antigen.Skyrim.Caches;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Antigen.Skyrim.Record.Cell.Interior;

public class ShowSkyAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<IFormLinkNullableGetter<IRegionGetter>> WrongRegion = MutagenTopicBuilder.FromDiscussion(
            391,
            "Weather/Sky Region Mismatch",
            Severity.Warning)
        .WithFormatting<IFormLinkNullableGetter<IRegionGetter>>("The cell has sky enabled but its sky/weather from region {0} does not match the regions of the exterior cells that lead to it");

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

        if (!cell.Flags.HasFlag(Mutagen.Bethesda.Skyrim.Cell.Flag.ShowSky)) return;

        var cellSkyAndWeatherFromRegion = cell.SkyAndWeatherFromRegion;
        if (cellSkyAndWeatherFromRegion.IsNull)
        {
            param.AddTopic(ShowSkyWithoutRegion.Format());
        }

        var exteriorCellCache = param.ResolveCache<IExteriorCellCache>();
        var exteriorCells = cell.GetExteriorDoorsGoingIntoInteriorRecursively(param.LinkCache)
            .Select(door => door.GetCell(param.LinkCache, exteriorCellCache))
            .WhereNotNull()
            .ToArray();

        var regionsInExteriorExits = exteriorCells
            .SelectMany(c => c.Regions ?? [])
            .ToHashSet();

        if (!regionsInExteriorExits.Contains(cellSkyAndWeatherFromRegion)) {
            param.AddTopic(
                WrongRegion.Format(cellSkyAndWeatherFromRegion),
                ("Exterior Cells", exteriorCells),
                ("Exterior Cell Regions", regionsInExteriorExits));
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
