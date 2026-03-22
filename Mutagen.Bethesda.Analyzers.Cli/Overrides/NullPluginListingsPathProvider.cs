using Mutagen.Bethesda.Plugins.Order.DI;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Cli.Overrides;

internal class NullPluginListingsPathProvider : IPluginListingsPathProvider
{
    public FilePath? Get(GameRelease release) => string.Empty;
}
