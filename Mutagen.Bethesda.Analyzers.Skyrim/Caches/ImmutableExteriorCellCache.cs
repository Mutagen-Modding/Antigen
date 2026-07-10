using System.Collections.Concurrent;
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

public class ImmutableExteriorCellCache(ILinkCache linkCache) : IExteriorCellCache
{
    private WorldspaceLookup CreateLookupForWorld(FormKey worldspace)
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

    private readonly ConcurrentDictionary<FormKey, Lazy<WorldspaceLookup>> _worldLookup = new();

    IFormLinkGetter<ICellGetter> GetExterior(FormKey worldspace, P2Int grid)
    {
        var lookup = _worldLookup.GetOrAdd(worldspace, static (w, cache) => new Lazy<WorldspaceLookup>(() =>
        {
            return cache.CreateLookupForWorld(w);
        }), this);

        if (lookup.Value.TryGetValue(grid, out var cell))
            return cell;
        return FormLink<ICellGetter>.Null;
    }

    public IFormLinkGetter<ICellGetter> GetExterior(IFormLinkGetter<IWorldspaceGetter> worldspace, P2Int grid) =>
        GetExterior(worldspace.FormKey, grid);

    public IFormLinkGetter<ICellGetter> GetExterior(IWorldspaceGetter worldspace, P2Int grid) =>
        GetExterior(worldspace.FormKey, grid);
}
