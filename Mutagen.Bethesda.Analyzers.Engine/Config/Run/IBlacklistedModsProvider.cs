using Mutagen.Bethesda.Plugins;

namespace Mutagen.Bethesda.Analyzers.Config.Run;

/// <summary>
/// Provides a mechanism to disallow certain mods from being analyzed.
/// </summary>
public interface IBlacklistedModsProvider
{
    /// <summary>
    /// Returns true if the mod is blacklisted and should not be analyzed.
    /// </summary>
    /// <param name="modKey">Mod to check</param>
    /// <returns>True if the mod is blacklisted</returns>
    public bool IsBlacklisted(ModKey modKey);
}
