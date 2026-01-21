using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Headers;
using Mutagen.Bethesda.Plugins.Meta;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Records.Mapping;
using Noggog.WorkEngine;
using RecordTypes = Mutagen.Bethesda.Plugins.Records.Internals.RecordTypes;

namespace Mutagen.Bethesda.Analyzers.Drivers.RecordFrame;

public class ByGenericTypeRecordFrameIsolatedDriver<TMajor> : IIsolatedRecordFrameAnalyzerDriver
    where TMajor : class, IMajorRecordGetter
{
    private readonly IWorkDropoff _dropoff;
    private readonly IIsolatedRecordFrameAnalyzer<TMajor>[] _isolatedRecordFrameAnalyzers;

    public bool Applicable => _isolatedRecordFrameAnalyzers.Length > 0;

    public IEnumerable<IAnalyzer> Analyzers => _isolatedRecordFrameAnalyzers;

    public RecordType TargetType => MajorRecordTypeLookup<TMajor>.RecordType;

    public ByGenericTypeRecordFrameIsolatedDriver(
        IAnalyzerProvider<IIsolatedRecordFrameAnalyzer<TMajor>> isolatedRecordFrameAnalyzerProvider,
        IWorkDropoff dropoff)
    {
        _isolatedRecordFrameAnalyzers = isolatedRecordFrameAnalyzerProvider.GetAnalyzers().ToArray();
        _dropoff = dropoff;
    }

    public async Task Drive(IsolatedDriverParams driverParams, MajorRecordFrame frame)
    {
        if (driverParams.CancellationToken.IsCancellationRequested) return;

        string? editorId = null;
        if (frame.TryFindSubrecord(RecordTypes.EDID, out var edid))
        {
            editorId = edid.AsString(GameConstants.Get(driverParams.TargetMod.GameRelease).Encodings.NonLocalized);
        }
        var formKey = new FormKey(driverParams.TargetMod.ModKey, frame.FormID.Id(driverParams.TargetMod.MasterStyle));

        var reportContext = new ReportContextParameters(driverParams.LinkCache);
        var param = new IsolatedRecordFrameAnalyzerParams<TMajor>(
            driverParams.ReportDropbox,
            driverParams.TargetMod,
            new FormLinkInformation(formKey, typeof(TMajor)),
            reportContext,
            frame);

        await Task.WhenAll(_isolatedRecordFrameAnalyzers.Select(analyzer =>
        {
            return _dropoff.EnqueueAndWait(() =>
            {
                analyzer.AnalyzeRecord(param with
                {
                    AnalyzerType = analyzer.GetType()
                });
            }, driverParams.CancellationToken);
        }));
    }
}
