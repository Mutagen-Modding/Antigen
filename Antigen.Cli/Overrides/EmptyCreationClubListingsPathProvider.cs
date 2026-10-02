using Mutagen.Bethesda.Plugins.Order.DI;
using Noggog;

namespace Antigen.Cli.Overrides;

internal class EmptyCreationClubListingsPathProvider : ICreationClubListingsPathProvider
{
    public FilePath? Path => string.Empty;
}
