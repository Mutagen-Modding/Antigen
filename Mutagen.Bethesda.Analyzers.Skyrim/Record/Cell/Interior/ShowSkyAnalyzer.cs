using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;


namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Interior;

public class ShowSkyAnalyzer : IIsolatedRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition ShowSkyWithoutRegion = MutagenTopicBuilder.FromDiscussion(
            394,
            "ShowSky with no region",
            Severity.Warning)
        .WithoutFormatting("Cell has ShowSky flag but no region assigned");

    IEnumerable<TopicDefinition> IAnalyzer.Topics => [ShowSkyWithoutRegion];

    void IIsolatedRecordAnalyzer<ICellGetter>.AnalyzeRecord(IsolatedRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;

        if (cell.IsExteriorCell()) return;
 
        if (!cell.Flags.HasFlag(Bethesda.Skyrim.Cell.Flag.ShowSky)) return;

        if (cell.Regions is null || cell.Regions?.Count < 1)
        {
            param.AddTopic(ShowSkyWithoutRegion.Format());
        }
    }

    IEnumerable<Func<ICellGetter, object?>> IIsolatedRecordAnalyzer<ICellGetter>.FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.Regions;
    }
}

