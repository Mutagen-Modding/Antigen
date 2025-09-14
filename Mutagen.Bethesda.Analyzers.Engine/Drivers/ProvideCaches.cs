using System.Collections.Concurrent;
using Mutagen.Bethesda.Analyzers.SDK.Caches;
using Mutagen.Bethesda.Plugins.Cache;

namespace Mutagen.Bethesda.Analyzers.Drivers;

public class ProvideCaches : IProvideCaches
{
    private readonly ILinkCache _linkCache;
    private readonly IReadOnlyDictionary<Type, ICacheConstructor> _cacheConstructors;
    private readonly ConcurrentDictionary<Type, object> _caches = new();

    public ProvideCaches(
        ILinkCache linkCache,
        ICacheConstructor[] cacheConstructors)
    {
        _linkCache = linkCache;
        _cacheConstructors = cacheConstructors
            .ToDictionary(x => x.CacheType, x => x);
    }

    public TAnalyzerCache Resolve<TAnalyzerCache>()
    {
        return (TAnalyzerCache)_caches.GetOrAdd(typeof(TAnalyzerCache), k =>
        {
            if (!_cacheConstructors.TryGetValue(k, out var constructor))
            {
                throw new ArgumentException("Could not construct cache of type " + k.FullName);
            }

            return constructor.Construct( _linkCache, this);
        });
    }
}
