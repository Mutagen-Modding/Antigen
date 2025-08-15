using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim.Records.Assets.VoiceType;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Util;

public static class VoiceTypeAssetLookupUtil
{
    private static readonly Lock Lock = new();
    private static VoiceTypeAssetLookup? _voiceTypeAssetLookup;

    public static VoiceTypeAssetLookup GetVoiceTypeAssetLookup(ILinkCache linkCache)
    {
        // TODO: Replace with something that is updated when the link cache is updated
        lock (Lock)
        {
            if (_voiceTypeAssetLookup is null)
            {
                var immutableAssetLinkCache = linkCache.CreateImmutableAssetLinkCache();

                _voiceTypeAssetLookup = new VoiceTypeAssetLookup();
                _voiceTypeAssetLookup.Prep(immutableAssetLinkCache);
            }
        }

        return _voiceTypeAssetLookup;
    }
}
