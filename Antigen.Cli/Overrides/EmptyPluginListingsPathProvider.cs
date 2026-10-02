using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Order.DI;
using Noggog;

namespace Antigen.Cli.Overrides;

internal class EmptyPluginListingsPathProvider : IPluginListingsPathProvider
{
    public FilePath? Get(GameRelease release) => string.Empty;
}
