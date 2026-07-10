using Mutagen.Bethesda.Analyzers.SDK.Caches;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Caches;

using WorldspaceLookup = IReadOnlyDictionary<P2Int, IFormLinkGetter<ICellGetter>>;

public interface IExteriorCellCache
{
    public IFormLinkGetter<ICellGetter> GetExterior(IFormLinkGetter<IWorldspaceGetter> worldspace, P2Int grid);
    public IFormLinkGetter<ICellGetter> GetExterior(IWorldspaceGetter worldspace, P2Int grid);
};

public class ExteriorCellCacheProvider : ICacheConstructor
{
    public Type CacheType => typeof(IExteriorCellCache);

    public object Construct(ILinkCache linkCache, IProvideCaches provideCaches)
    {
        return new ImmutableExteriorCellCache(linkCache);
    }
}

public class ImmutableExteriorCellCache(ILinkCache linkCache) : IExteriorCellCache
{
    private readonly LazyEntryCache<FormKey, WorldspaceLookup> _worldLookup =
        new(worldspace => CreateLookupForWorld(linkCache, worldspace));

    private static WorldspaceLookup CreateLookupForWorld(ILinkCache linkCache, FormKey worldspace)
    {
        var lookup = new Dictionary<P2Int, IFormLinkGetter<ICellGetter>>();

        foreach (var worldspaceOverride in linkCache.ResolveAll<IWorldspaceGetter>(worldspace))
        {
            foreach (var block in worldspaceOverride.SubCells)
            {
                foreach (var subBlock in block.Items)
                {
                    foreach (var cell in subBlock.Items)
                    {
                        if (cell.Grid != null)
                        {
                            lookup[cell.Grid.Point] = cell.ToLink();
                        }
                    }
                }
            }
        }
        return lookup;
    }

    private IFormLinkGetter<ICellGetter> GetExterior(FormKey worldspace, P2Int grid)
    {
        var lookup = _worldLookup.GetOrAdd(worldspace);

        if (lookup.TryGetValue(grid, out var cell))
            return cell;
        return FormLink<ICellGetter>.Null;
    }

    public IFormLinkGetter<ICellGetter> GetExterior(IFormLinkGetter<IWorldspaceGetter> worldspace, P2Int grid) =>
        GetExterior(worldspace.FormKey, grid);

    public IFormLinkGetter<ICellGetter> GetExterior(IWorldspaceGetter worldspace, P2Int grid) =>
        GetExterior(worldspace.FormKey, grid);
}
