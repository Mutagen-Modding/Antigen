using Mutagen.Bethesda.Analyzers.SDK.Caches;
using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;

namespace Mutagen.Bethesda.Analyzers.SDK.Analyzers;

/// <summary>
/// Object containing all the parameters available for a <see cref="IContextualAnalyzer"/>
/// </summary>
public readonly struct ContextualAnalyzerParams
{
    public Type? AnalyzerType { get; init; }
    private readonly IReportDropbox _reportDropbox;
    private readonly ReportContextParameters _parameters;
    private readonly IProvideCaches _provideCaches;

    /// <summary>
    /// Link Cache to use during analysis
    /// </summary>
    public readonly ILinkCache LinkCache;

    /// <summary>
    /// Load Order to use during analysis
    /// </summary>
    public readonly ILoadOrderGetter<IModListingGetter<IModGetter>> LoadOrder;

    public ContextualAnalyzerParams(
        ILinkCache linkCache,
        ILoadOrderGetter<IModListingGetter<IModGetter>> loadOrder,
        IReportDropbox reportDropbox,
        IProvideCaches provideCaches,
        ReportContextParameters parameters)
    {
        LinkCache = linkCache;
        LoadOrder = loadOrder;
        _reportDropbox = reportDropbox;
        _parameters = parameters;
        _provideCaches = provideCaches;
    }

    /// <summary>
    /// Reports a topic to the engine
    /// </summary>
    public void AddTopic(
        ModKey mod,
        IMajorRecordIdentifierGetter record,
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
