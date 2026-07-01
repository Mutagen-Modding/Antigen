using Mutagen.Bethesda.Plugins.Order.DI;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Cli.Overrides;

internal class EmptyPluginListingsPathProvider : IPluginListingsPathProvider
{
    public FilePath? Get(GameRelease release) => string.Empty;
}
