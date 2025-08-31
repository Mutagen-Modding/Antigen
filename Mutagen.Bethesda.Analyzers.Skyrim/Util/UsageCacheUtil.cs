using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Cache.Internals.Implementations;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Util;

public static class UsageCacheUtil
{
    private static readonly Lock Lock = new();
    private static ImmutableLoadOrderLinkUsageCache? _usageCache;

    public static ImmutableLoadOrderLinkUsageCache GetUsageCache(ILinkCache linkCache)
    {
        // TODO: Replace with something that is updated when the link cache is updated
        lock (Lock)
        {
            if (_usageCache is null)
            {
                _usageCache = new ImmutableLoadOrderLinkUsageCache(linkCache);
            }
        }

        return _usageCache;
    }
}
