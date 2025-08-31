using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;

namespace Mutagen.Bethesda.Analyzers.SDK.Analyzers;

public readonly struct ContextualRecordAnalyzerParams<TMajor>
    where TMajor : IMajorRecordGetter
{
    public Type? AnalyzerType { get; init; }
    public readonly ILinkCache LinkCache;
    public readonly ILoadOrderGetter<IModListingGetter<IModGetter>> LoadOrder;
    private readonly IModGetter _mod;
    public readonly TMajor Record;
    private readonly IReportDropbox _reportDropbox;
    private readonly ReportContextParameters _parameters;

    public ContextualRecordAnalyzerParams(ILinkCache linkCache,
        ILoadOrderGetter<IModListingGetter<IModGetter>> loadOrder,
        IModGetter mod,
        TMajor record,
        IReportDropbox reportDropbox)
    {
        LinkCache = linkCache;
        LoadOrder = loadOrder;
        _mod = mod;
        Record = record;
        _reportDropbox = reportDropbox;
        _parameters = new ReportContextParameters(linkCache);
    }

    public void AddTopic(
        IFormattedTopicDefinition formattedTopicDefinition,
        params (string Name, object Value)[] metaData)
    {
        _reportDropbox.Dropoff(
            _parameters,
            _mod.ModKey,
            Record,
            Topic.Create(formattedTopicDefinition, AnalyzerType, metaData));
    }

    public void AddTopic(
        ModKey mod,
        TMajor record,
        IFormattedTopicDefinition formattedTopicDefinition,
        params (string Name, object Value)[] metaData)
    {
        _reportDropbox.Dropoff(
            _parameters,
            mod,
            record,
            Topic.Create(formattedTopicDefinition, AnalyzerType, metaData));
    }
}
