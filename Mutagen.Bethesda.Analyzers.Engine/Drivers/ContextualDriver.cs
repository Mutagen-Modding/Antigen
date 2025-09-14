using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers.Drivers;

public class ContextualDriver : IContextualDriver
{
    private readonly IWorkDropoff _dropoff;
    private readonly IContextualAnalyzer[] _contextualAnalyzers;
    public bool Applicable => true;
    public IEnumerable<IAnalyzer> Analyzers => _contextualAnalyzers;

    public ContextualDriver(
        IAnalyzerProvider<IContextualAnalyzer> contextualAnalyzerProvider,
        IWorkDropoff dropoff)
    {
        _contextualAnalyzers = contextualAnalyzerProvider.GetAnalyzers().ToArray();
        _dropoff = dropoff;
    }

    public async Task Drive(ContextualDriverParams driverParams)
    {
        if (driverParams.CancellationToken.IsCancellationRequested) return;
        if (_contextualAnalyzers.Length == 0) return;
        var reportContext = new ReportContextParameters(driverParams.LinkCache);
        var analyzerParams = new ContextualAnalyzerParams(
            driverParams.LinkCache,
            driverParams.LoadOrder,
            driverParams.ReportDropbox,
            driverParams.ProvideCaches,
            reportContext);
        if (driverParams.CancellationToken.IsCancellationRequested) return;
        await Task.WhenAll(_contextualAnalyzers.Select(contextualAnalyzer =>
        {
            return _dropoff.EnqueueAndWait(() =>
            {
                contextualAnalyzer.Analyze(analyzerParams with
                {
                    AnalyzerType = contextualAnalyzer.GetType()
                });
            }, driverParams.CancellationToken);
        }));
    }
}
