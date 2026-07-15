using System.IO.Abstractions;
using Antigen.Resources.Comparer;
using DynamicData;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Services;

public record struct ModInfo(
    ModKey ModKey,
    string? Author,
    string? Description,
    bool Localization,
    int FormVersion,
    ModKey[] DirectMasters)
{
    public ModInfo(ModKey modKey) : this(modKey, null, null, false, -1, []) {}
}
public sealed class SkyrimModInfoProvider : IModInfoProvider<ISkyrimModGetter>
{

    public ModInfo GetModInfo(ModKey modKey, MutagenFrame modHeaderFrame)
    {
        return GetModInfo(modKey, SkyrimModHeader.CreateFromBinary(modHeaderFrame));
    }
    public ModInfo? GetModInfo(IModGetter mod)
    {
        return mod is ISkyrimModGetter skyrimMod ? GetModInfo(skyrimMod.ModKey, skyrimMod.ModHeader) : null;
    }
    public ModInfo GetModInfo(ISkyrimModGetter mod)
    {
        return GetModInfo(mod.ModKey, mod.ModHeader);
    }
    public ModInfo? GetModInfo(string filePath, IFileSystem fileSystem, GameRelease gameRelease)
    {
        if (!fileSystem.File.Exists(filePath)) return null;

        var fileName = fileSystem.Path.GetFileName(filePath);

        var modKey = ModKey.FromFileName(fileName);
        var binaryReadParameters = new BinaryReadParameters { FileSystem = fileSystem };
        var modPath = new ModPath(modKey, filePath);
        var parsingMeta = ParsingMeta.Factory(binaryReadParameters, gameRelease, modPath);
        var stream = new MutagenBinaryReadStream(filePath, parsingMeta);
        using var frame = new MutagenFrame(stream);
        return GetModInfo(modKey, frame);
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

    public uint GetRecordCount(ISkyrimModGetter mod)
    {
        return mod.ModHeader.Stats.NumRecords;
    }

    public uint GetRecordCount(IModGetter mod)
    {
        return mod is ISkyrimModGetter skyrimModGetter
            ? GetRecordCount(skyrimModGetter)
            : 0;
    }
    public static ModInfo GetModInfo(ModKey modKey, ISkyrimModHeaderGetter modHeader)
    {
        return new ModInfo(
            modKey,
            modHeader.Author,
            modHeader.Description,
            (modHeader.Flags & SkyrimModHeader.HeaderFlag.Localized) != 0,
            modHeader.FormVersion,
            modHeader.MasterReferences.Select(master => master.Master).ToArray());
    }
}