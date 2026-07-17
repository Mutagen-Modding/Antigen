using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Mutagen.Bethesda.Plugins.Records;

namespace Antigen.Services.Game;

public interface IModInfoProvider
{
    /// <summary>
    ///     Returns ModInfo for a mod
    /// </summary>
    /// <param name="modKey">ModKey of the mod to get ModInfo for</param>
    /// <param name="modHeaderFrame">Frame containing the mod header</param>
    /// <returns></returns>
    ModInfo GetModInfo(ModKey modKey, MutagenFrame modHeaderFrame);

    /// <summary>
    ///     Returns ModInfo for a mod
    /// </summary>
    /// <param name="mod"></param>
    /// <returns></returns>
    ModInfo? GetModInfo(IModGetter mod);

    /// <summary>
    ///     Returns ModInfo for a mod file link
    /// </summary>
    /// <param name="filePath">Path to the mod file</param>
    /// <param name="fileSystem">File system to use for file access</param>
    /// <param name="gameRelease">Game release to use for mod info</param>
    /// <returns>ModInfo of the mod if the file link points to a valid mod, null otherwise</returns>
    ModInfo? GetModInfo(string filePath, IFileSystem fileSystem, GameRelease gameRelease);

    /// <summary>
    ///     Build dictionary masterInfos with all masters of a single plugin recursively
    /// </summary>
    /// <param name="modInfos">List of all mod infos</param>
    /// <returns>Dictionary of ModKey to a tuple of all masters and whether all masters are valid</returns>
    Dictionary<ModKey, (HashSet<ModKey> Masters, bool Valid)> GetMasterInfos(IReadOnlyList<ModInfo> modInfos);

    /// <summary>
    ///     Returns the number of records in a mod
    /// </summary>
    /// <param name="mod">Mod to get number of records for</param>
    /// <returns>Approximate number of records in a mod - may not be fully accurate!</returns>
    uint GetRecordCount(IModGetter mod);
}
public interface IModInfoProvider<in TModGetter> : IModInfoProvider
    where TModGetter : class, IModGetter
{
    /// <summary>
    ///     Returns ModInfo for a mod
    /// </summary>
    /// <param name="mod">Mod to get ModInfos for</param>
    /// <returns>ModInfo of the mod</returns>
    ModInfo GetModInfo(TModGetter mod);

    /// <summary>
    ///     Returns the number of records in a mod
    /// </summary>
    /// <param name="mod">Mod to get number of records for</param>
    /// <returns>Approximate number of records in a mod - may not be fully accurate!</returns>
    uint GetRecordCount(TModGetter mod);
}