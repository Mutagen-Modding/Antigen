using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Plugins.Records;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers.Drivers.Records;

public class ByGenericTypeRecordIsolatedDriver<TMajor> : IIsolatedDriver
    where TMajor : class, IMajorRecordGetter
{
    private readonly IWorkDropoff _dropoff;
    private readonly IIsolatedRecordAnalyzer<TMajor>[] _isolatedRecordAnalyzers;

    public bool Applicable => _isolatedRecordAnalyzers.Length > 0;

    public IEnumerable<IAnalyzer> Analyzers => _isolatedRecordAnalyzers;

    public ByGenericTypeRecordIsolatedDriver(
        IAnalyzerProvider<IIsolatedRecordAnalyzer<TMajor>> isolatedRecordAnalyzerProvider,
        IWorkDropoff dropoff)
    {
        _isolatedRecordAnalyzers = isolatedRecordAnalyzerProvider.GetAnalyzers().ToArray();
        _dropoff = dropoff;
    }

    public async Task Drive(IsolatedDriverParams driverParams)
    {
        if (driverParams.CancellationToken.IsCancellationRequested) return;
        if (_isolatedRecordAnalyzers.Length == 0) return;
        var reportContext = new ReportContextParameters(driverParams.LinkCache);

        await Task.WhenAll(driverParams.TargetMod
            .EnumerateMajorRecords<TMajor>()
            .Where(x => !x.IsDeleted)
            .SelectMany(rec =>
            {
                var isolatedParam = new IsolatedRecordAnalyzerParams<TMajor>(
                    driverParams.TargetMod.ModKey,
                    rec,
                    reportContext,
                    driverParams.ReportDropbox);
                return _isolatedRecordAnalyzers.Select(analyzer =>
                {
                    return _dropoff.EnqueueAndWait(() =>
                    {
                        analyzer.AnalyzeRecord(isolatedParam with
                        {
                            AnalyzerType = analyzer.GetType()
                        });
                    }, driverParams.CancellationToken);
                });
            }));
    }
}
