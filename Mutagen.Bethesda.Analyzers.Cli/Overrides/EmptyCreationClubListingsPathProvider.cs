using Mutagen.Bethesda.Plugins.Order.DI;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Cli.Overrides;

internal class EmptyCreationClubListingsPathProvider : ICreationClubListingsPathProvider
{
    public FilePath? Path => string.Empty;
}
