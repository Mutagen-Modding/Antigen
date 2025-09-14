using Mutagen.Bethesda.Analyzers.SDK.Caches;
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
    private readonly ModKey _modKey;
    public readonly TMajor Record;
    private readonly IReportDropbox _reportDropbox;
    private readonly IProvideCaches _provideCaches;
    private readonly ReportContextParameters _parameters;

    public ContextualRecordAnalyzerParams(ILinkCache linkCache,
        ILoadOrderGetter<IModListingGetter<IModGetter>> loadOrder,
        ModKey modKey,
        TMajor record,
        IReportDropbox reportDropbox,
        IProvideCaches provideCaches)
    {
        LinkCache = linkCache;
        LoadOrder = loadOrder;
        _modKey = modKey;
        Record = record;
        _reportDropbox = reportDropbox;
        _provideCaches = provideCaches;
        _parameters = new ReportContextParameters(linkCache);
    }

    public void AddTopic(
        IFormattedTopicDefinition formattedTopicDefinition,
        params (string Name, object Value)[] metaData)
    {
        _reportDropbox.Dropoff(
            _parameters,
            _modKey,
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

    public TAnalyzerCache ResolveCache<TAnalyzerCache>()
    {
        return _provideCaches.Resolve<TAnalyzerCache>();
    }
}
