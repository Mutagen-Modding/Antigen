using Mutagen.Bethesda.Plugins;

namespace Mutagen.Bethesda.Analyzers.Config.Run;

public class InjectedBlacklistedModsProvider(IEnumerable<ModKey> blacklistedMods) : IBlacklistedModsProvider
{
    private readonly List<ModKey> _blacklistedMods = blacklistedMods.ToList();

    public bool IsBlacklisted(ModKey modKey)
    {
        return _blacklistedMods.Contains(modKey);
    }
}
