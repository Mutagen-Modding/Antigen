using Mutagen.Bethesda.Analyzers.SDK.Caches;
using Mutagen.Bethesda.Plugins.Cache;

namespace Mutagen.Bethesda.Analyzers.Drivers;

public class ProvideCaches : IProvideCaches
{
    private readonly IReadOnlyDictionary<Type, Lazy<object>> _caches;

    public ProvideCaches(
        ILinkCache linkCache,
        ICacheConstructor[] cacheConstructors)
    {
        _caches = cacheConstructors.ToDictionary(
            x => x.CacheType, x => new Lazy<object>(() =>
            {
                return x.Construct(linkCache, this);
            }, LazyThreadSafetyMode.ExecutionAndPublication));
    }

    public TAnalyzerCache Resolve<TAnalyzerCache>()
    {
        if (!_caches.TryGetValue(typeof(TAnalyzerCache), out var cache) || cache.Value is not TAnalyzerCache analyzerCache)
            throw new ArgumentException("Could not construct cache of type " + typeof(TAnalyzerCache).FullName);
        return analyzerCache;
    }
}
