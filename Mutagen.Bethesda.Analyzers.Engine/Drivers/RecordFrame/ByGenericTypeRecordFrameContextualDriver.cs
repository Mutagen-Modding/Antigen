using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Headers;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Records.Mapping;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers.Drivers.RecordFrame;

public class ByGenericTypeRecordFrameContextualDriver<TMajor> : IContextualRecordFrameAnalyzerDriver
    where TMajor : class, IMajorRecordGetter
{
    private readonly IWorkDropoff _dropoff;
    private readonly IContextualRecordFrameAnalyzer<TMajor>[] _contextualRecordFrameAnalyzers;

    public bool Applicable => _contextualRecordFrameAnalyzers.Length > 0;

    public IEnumerable<IAnalyzer> Analyzers => _contextualRecordFrameAnalyzers;

    public RecordType TargetType => MajorRecordTypeLookup<TMajor>.RecordType;

    public ByGenericTypeRecordFrameContextualDriver(
        IWorkDropoff dropoff,
        IAnalyzerProvider<IContextualRecordFrameAnalyzer<TMajor>> contextualRecordFrameAnalyzerProvider)
    {
        _dropoff = dropoff;
        _contextualRecordFrameAnalyzers = contextualRecordFrameAnalyzerProvider.GetAnalyzers().ToArray();
    }

    public async Task Drive(ContextualDriverParams driverParams, MajorRecordFrame frame)
    {
        if (driverParams.CancellationToken.IsCancellationRequested) return;
        var reportContext = new ReportContextParameters(driverParams.LinkCache);
        var param = new ContextualRecordFrameAnalyzerParams<TMajor>(
            driverParams.LinkCache,
            frame);

        await Task.WhenAll(_contextualRecordFrameAnalyzers.Select(analyzer =>
        {
            return _dropoff.EnqueueAndWait(() =>
            {
                analyzer.AnalyzeRecord(param);
            }, driverParams.CancellationToken);
        }));
    }
}
