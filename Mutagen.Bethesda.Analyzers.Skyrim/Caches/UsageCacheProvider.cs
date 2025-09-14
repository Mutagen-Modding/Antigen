using Mutagen.Bethesda.Analyzers.SDK.Caches;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Cache.Internals.Implementations;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Caches;

public class UsageCacheProvider : ICacheConstructor
{
    public Type CacheType => typeof(ILinkUsageCache);

    public object Construct(ILinkCache linkCache, IProvideCaches provideCaches)
    {
        return new ImmutableLoadOrderLinkUsageCache(linkCache);
    }
}
