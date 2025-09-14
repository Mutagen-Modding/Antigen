using Mutagen.Bethesda.Analyzers.SDK.Caches;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim.Records.Assets.VoiceType;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Caches;

public class VoiceTypeAssetLookupProvider : ICacheConstructor
{
    public Type CacheType => typeof(VoiceTypeAssetLookup);

    public object Construct(
        ILinkCache linkCache,
        IProvideCaches provideCaches)
    {
        var immutableAssetLinkCache = linkCache.CreateImmutableAssetLinkCache();

        var ret = new VoiceTypeAssetLookup();
        ret.Prep(immutableAssetLinkCache);
        return ret;
    }
}
