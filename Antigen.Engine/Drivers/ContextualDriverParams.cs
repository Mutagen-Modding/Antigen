using Antigen.SDK.Caches;
using Antigen.SDK.Drops;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;

namespace Antigen.Drivers;

public readonly struct ContextualDriverParams
{
    public readonly ILinkCache LinkCache;
    public readonly ILoadOrderGetter<IModListingGetter<IModGetter>> LoadOrder;
    public readonly IReportDropbox ReportDropbox;
    public readonly CancellationToken CancellationToken;
    public readonly IProvideCaches ProvideCaches;

    public ContextualDriverParams(
        ILinkCache linkCache,
        ILoadOrderGetter<IModListingGetter<IModGetter>> loadOrder,
        IReportDropbox reportDropbox,
        IProvideCaches provideCaches,
        CancellationToken cancellationToken)
    {
        LinkCache = linkCache;
        LoadOrder = loadOrder;
        ReportDropbox = reportDropbox;
        CancellationToken = cancellationToken;
        ProvideCaches = provideCaches;
    }
}
