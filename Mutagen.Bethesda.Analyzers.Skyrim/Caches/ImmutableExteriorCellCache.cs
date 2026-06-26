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
    private readonly Dictionary<FormKey, Dictionary<P2Int, IFormLinkGetter<ICellGetter>>> _worldLookup = [];

    private Dictionary<P2Int, IFormLinkGetter<ICellGetter>>? GetLookupForWorld(FormKey world)
    {
        if (_worldLookup.TryGetValue(world, out var lookup)) return lookup;

        lookup = [];

        foreach (var worldspaceOverride in linkCache.ResolveAll<IWorldspaceGetter>(world))
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
        _worldLookup[world] = lookup;
        return lookup;
    }

    IFormLinkGetter<ICellGetter> GetExterior(FormKey worldspace, P2Int grid)
    {
        if (GetLookupForWorld(worldspace)?.TryGetValue(grid, out var cell) ?? false)
            return cell;
        return FormLink<ICellGetter>.Null;
    }

    public IFormLinkGetter<ICellGetter> GetExterior(IFormLinkGetter<IWorldspaceGetter> worldspace, P2Int grid) =>
        GetExterior(worldspace.FormKey, grid);

    public IFormLinkGetter<ICellGetter> GetExterior(IWorldspaceGetter worldspace, P2Int grid) =>
        GetExterior(worldspace.FormKey, grid);
}
