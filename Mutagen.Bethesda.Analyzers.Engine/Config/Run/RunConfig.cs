using Mutagen.Bethesda.Plugins;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Config.Run;

public interface IRunConfigLookup
{
    DirectoryPath? DataDirectoryPath { get; }
    FilePath? OutputFilePath { get; }
    IEnumerable<ModKey>? LoadOrderSetToMods { get; }
    IEnumerable<ModKey>? BlacklistMods { get; }
    bool PrintMetadata { get; set; }
}

public interface IRunConfig : IRunConfigLookup
{
    void OverrideDataDirectory(DirectoryPath path);
    void OverrideOutputFilePath(FilePath filePath);
    void OverrideLoadOrderSetToMods(IEnumerable<ModKey> mods);
    void OverrideBlacklistMods(IEnumerable<ModKey> mods);
    void OverridePrintMetadata(bool printMetadata);
}

public class RunConfig : IRunConfig
{
    public DirectoryPath? DataDirectoryPath { get; private set; }
    public FilePath? OutputFilePath { get; private set; }
    public IEnumerable<ModKey>? LoadOrderSetToMods { get; private set; }
    public IEnumerable<ModKey>? BlacklistMods { get; private set; }
    public bool PrintMetadata { get; set; }

    public void OverrideDataDirectory(DirectoryPath path) => DataDirectoryPath = path;
    public void OverrideOutputFilePath(FilePath filePath) => OutputFilePath = filePath;
    public void OverrideLoadOrderSetToMods(IEnumerable<ModKey> mods) => LoadOrderSetToMods = mods;
    public void OverrideBlacklistMods(IEnumerable<ModKey> mods) => BlacklistMods = mods;
    public void OverridePrintMetadata(bool printMetadata) => PrintMetadata = printMetadata;
}
