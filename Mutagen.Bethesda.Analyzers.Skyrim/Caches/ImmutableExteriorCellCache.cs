using System.Collections.Concurrent;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Caches;

public interface IExteriorCellCache
{
    public IFormLinkGetter<ICellGetter> GetExterior(IFormLinkGetter<IWorldspaceGetter> worldspace, P2Int grid);
    public IFormLinkGetter<ICellGetter> GetExterior(IWorldspaceGetter worldspace, P2Int grid);
};

public class ImmutableExteriorCellCache(ILinkCache linkCache) : IExteriorCellCache
{
    private readonly ConcurrentDictionary<FormKey, Dictionary<P2Int, IFormLinkGetter<ICellGetter>>> _worldLookup = new();

    private Dictionary<P2Int, IFormLinkGetter<ICellGetter>> GetLookupForWorld(FormKey world)
    {
        return _worldLookup.GetOrAdd(world, static (w, cache) =>
        {
            var lookup = new Dictionary<P2Int, IFormLinkGetter<ICellGetter>>();

            foreach (var worldspaceOverride in cache.ResolveAll<IWorldspaceGetter>(w))
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
        }, linkCache);
    }

    IFormLinkGetter<ICellGetter> GetExterior(FormKey worldspace, P2Int grid)
    {
        if (GetLookupForWorld(worldspace).TryGetValue(grid, out var cell))
            return cell;
        return FormLink<ICellGetter>.Null;
    }

    public IFormLinkGetter<ICellGetter> GetExterior(IFormLinkGetter<IWorldspaceGetter> worldspace, P2Int grid) =>
        GetExterior(worldspace.FormKey, grid);

    public IFormLinkGetter<ICellGetter> GetExterior(IWorldspaceGetter worldspace, P2Int grid) =>
        GetExterior(worldspace.FormKey, grid);
}
