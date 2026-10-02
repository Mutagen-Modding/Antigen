using Mutagen.Bethesda.Plugins.Cache;

namespace Antigen.SDK.Caches;

public interface ICacheConstructor
{
    public Type CacheType { get; }
    public object Construct(
        ILinkCache linkCache,
        IProvideCaches provideCaches);
}
