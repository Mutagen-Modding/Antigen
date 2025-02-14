using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Plugins;

namespace Mutagen.Bethesda.Analyzers.Config.Run;

public class ProcessBlacklistedMods(ILogger<ProcessBlacklistedMods> logger) : IConfigReaderProcessor<IRunConfig>
{
    public bool Process(IRunConfig config, IReadOnlyList<string> instructionParts, string value)
    {
        // environment.blacklisted_mods = <mod1>,<mod2>,...
        if (instructionParts.Count != 2) return false;

        if (instructionParts[0] is not "environment") return false;
        if (instructionParts[1] is not "blacklisted_mods") return false;

        var mods = value.Split(',');
        try
        {
            var modKeys = mods.Select(fileName => ModKey.FromFileName(fileName.Trim())).ToList();
            config.OverrideLoadOrderSetToMods(modKeys);
        }
        catch (ArgumentException e)
        {
            logger.LogError(e, "Error parsing ModKeys");
            return false;
        }

        return true;
    }
}
