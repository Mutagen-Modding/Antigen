using Mutagen.Bethesda.Analyzers.SDK.Caches;
using Mutagen.Bethesda.Plugins.Cache;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Caches;

public class ExteriorCellCacheProvider : ICacheConstructor
{
    public Type CacheType => typeof(IExteriorCellCache);

    public object Construct(ILinkCache linkCache, IProvideCaches provideCaches)
    {
        return new ImmutableExteriorCellCache(linkCache);
    }
}
