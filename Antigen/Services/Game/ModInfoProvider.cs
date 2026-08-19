using System.IO.Abstractions;
using Antigen.Resources.Comparer;
using DynamicData;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Masters;

namespace Antigen.Services.Game;

public record struct ModInfo(ModKey ModKey, ModKey[] DirectMasters);

public sealed class ModInfoProvider(ILogger<ModInfoProvider> logger) : ISingleton
{
    public ModInfo? GetModInfo(string filePath, IFileSystem fileSystem, GameRelease gameRelease)
    {
        if (!fileSystem.File.Exists(filePath)) return null;

        var modKey = ModKey.FromFileName(fileSystem.Path.GetFileName(filePath));

        try
        {
            var masters = MasterReferenceCollection.FromPath(new ModPath(modKey, filePath), gameRelease, fileSystem);
            return new ModInfo(modKey, masters.Masters.Select(master => master.Master).ToArray());
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Could not read masters for {Path}", filePath);
            return null;
        }
    }
    
    public Dictionary<ModKey, (HashSet<ModKey> Masters, bool Valid)> GetMasterInfos(IReadOnlyList<ModInfo> modInfos)
    {
        var sortedMods = modInfos
            .Order(new FuncComparer<ModInfo>((a, b) =>
            {
                // If one is a master of the other, it should come first
                if (a.DirectMasters.Contains(b.ModKey)) return 1;
                if (b.DirectMasters.Contains(a.ModKey)) return -1;

                //If neither is a master of the other, keep original order
                var aIndex = modInfos.IndexOf(a);
                var bIndex = modInfos.IndexOf(b);
                if (aIndex < 0 || bIndex < 0) return 0;

                return aIndex.CompareTo(bIndex);
            }))
            .ToArray();

        var masterInfos = new Dictionary<ModKey, (HashSet<ModKey> Masters, bool Valid)>();
        var modKeyIndices = sortedMods
            .Select((mod, i) => (mod.ModKey, i))
            .ToDictionary(x => x.ModKey, x => x.i);

        // Iterate in priority order
        foreach (var mod in sortedMods)
        {
            var masters = new HashSet<ModKey>(mod.DirectMasters);
            var valid = true;

            // Check that all masters are valid and compile list of all recursive masters
            foreach (var master in mod.DirectMasters)
            {
                if (masterInfos.TryGetValue(master, out var masterInfo) && masterInfo.Valid)
                {
                    foreach (var masterModKey in masterInfo.Masters)
                    {
                        masters.Add(masterModKey);
                    }
                    continue;
                }

                valid = false;
                break;
            }

            if (valid)
            {
                masters = masters.OrderBy(key => modKeyIndices[key]).ToHashSet();
            }
            else
            {
                masters.Clear();
            }

            masterInfos.Add(mod.ModKey, (masters, valid));
        }

        return masterInfos;
    }
}
