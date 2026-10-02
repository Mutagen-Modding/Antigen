using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Plugins;

namespace Antigen.Config.Run;

public class ProcessLoadOrderSetToMods(ILogger<ProcessLoadOrderSetToMods> logger) : IConfigReaderProcessor<IRunConfig>
{
    public bool Process(IRunConfig config, IReadOnlyList<string> instructionParts, string value)
    {
        // environment.load_order.set_to_mods = <mod1>,<mod2>,...
        if (instructionParts.Count != 3) return false;

        if (instructionParts[0] is not "environment") return false;
        if (instructionParts[1] is not "load_order") return false;
        if (instructionParts[2] is not "set_to_mods") return false;

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
