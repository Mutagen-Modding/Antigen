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

public class ImmutableExteriorCellCache : IExteriorCellCache
{
    public ImmutableExteriorCellCache(ILinkCache linkCache)
    {
        _worldLookup = linkCache.PriorityOrder.WinningOverrides<IWorldspaceGetter>()
            .ToDictionary(w => w.FormKey, w => new Lazy<IReadOnlyDictionary<P2Int, IFormLinkGetter<ICellGetter>>>(() =>
            {
                Console.WriteLine($"World ctor {w.EditorID}");
                var lookup = new Dictionary<P2Int, IFormLinkGetter<ICellGetter>>();

                foreach (var worldspaceOverride in linkCache.ResolveAll(w))
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
            }, LazyThreadSafetyMode.ExecutionAndPublication));
    }

    private readonly IReadOnlyDictionary<FormKey, Lazy<IReadOnlyDictionary<P2Int, IFormLinkGetter<ICellGetter>>>> _worldLookup;

    IFormLinkGetter<ICellGetter> GetExterior(FormKey worldspace, P2Int grid)
    {
        if (_worldLookup.TryGetValue(worldspace, out var lookup) && lookup.Value.TryGetValue(grid, out var cell))
            return cell;
        return FormLink<ICellGetter>.Null;
    }

    public IFormLinkGetter<ICellGetter> GetExterior(IFormLinkGetter<IWorldspaceGetter> worldspace, P2Int grid) =>
        GetExterior(worldspace.FormKey, grid);

    public IFormLinkGetter<ICellGetter> GetExterior(IWorldspaceGetter worldspace, P2Int grid) =>
        GetExterior(worldspace.FormKey, grid);
}
