using Mutagen.Bethesda.Plugins.Order.DI;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Cli.Overrides;

internal class NullCreationClubListingsPathProvider : ICreationClubListingsPathProvider
{
    public FilePath? Path => string.Empty;
}
