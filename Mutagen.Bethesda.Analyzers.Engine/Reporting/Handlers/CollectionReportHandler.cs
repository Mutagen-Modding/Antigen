using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Mutagen.Bethesda.Analyzers.Reporting.Handlers;

public class AnalyzerResult
{
    public required Topic Topic { get; init; }
    public required IMajorRecordIdentifierGetter Record { get; init; }
    public required ModKey ModKey { get; init; }
}

public class CollectionReportHandler : IReportHandler
{
    public List<AnalyzerResult> Results { get; } = [];

    public void Dropoff(ReportContextParameters parameters, ModKey sourceMod, IMajorRecordIdentifierGetter majorRecord, Topic topic)
    {
        Results.Add(new AnalyzerResult
        {
            Topic = topic,
            Record = majorRecord,
            ModKey = sourceMod,
        });
    }

    public void Dropoff(ReportContextParameters parameters, Topic topic)
    {
        Results.Add(new AnalyzerResult
        {
            Topic = topic,
            Record = null!,
            ModKey = ModKey.Null,
        });
    }
}
