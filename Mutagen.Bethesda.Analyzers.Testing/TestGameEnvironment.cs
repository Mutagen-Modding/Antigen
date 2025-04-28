using Mutagen.Bethesda.Assets.DI;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Testing;

public record TestGameEnvironment : IGameEnvironment, IDataDirectoryProvider
{
    DirectoryPath IDataDirectoryProvider.Path => DataFolderPath;
    public required DirectoryPath DataFolderPath { get; init; }
    public required GameRelease GameRelease { get; init; }
    public required FilePath LoadOrderFilePath { get; init; }
    public required FilePath? CreationClubListingsFilePath { get; init; }
    public required ILoadOrderGetter<IModListingGetter<IModGetter>> LoadOrder { get; init; }
    public required ILinkCache LinkCache { get; init; }
    public required IAssetProvider AssetProvider { get; init;  }

    public void Dispose()
    {
    }
}
