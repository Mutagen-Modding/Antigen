using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class CellExtensions
{
    public static IEnumerable<ILocationGetter> GetAllLocations(this ICellGetter cell, ILinkCache linkCache)
    {
        // Add all parent location form keys
        var cellLocations = new HashSet<ILocationGetter>();
        var location = cell.Location.TryResolve(linkCache);
        while (location is not null)
        {
            cellLocations.Add(location);
            location = location.ParentLocation.TryResolve(linkCache);
        }

        // Get world locations
        if (!cell.Flags.HasFlag(Cell.Flag.IsInteriorCell))
        {
            var context = linkCache.ResolveSimpleContext<ICellGetter>(cell.FormKey);
            var world = (IWorldspaceGetter?)context.Parent?.Record;
            if (world is not null)
            {
                foreach (var worldLocation in world.GetWorldLocations(linkCache))
                {
                    cellLocations.Add(worldLocation);
                }
            }
        }

        return cellLocations;
    }

    public static bool IsSettlementCell(this ICellGetter cell, ILinkCache linkCache)
    {
        if (!cell.IsInteriorCell()) return false;
        var locations = cell.GetAllLocations(linkCache).ToList();
        if (locations.Count == 0) return false;

        return locations.Exists(location => location.IsSettlementLocation());
    }

    public static bool IsInteriorCell(this ICellGetter cell)
    {
        return (cell.Flags & Cell.Flag.IsInteriorCell) != 0;
    }

    public static bool IsExteriorCell(this ICellGetter cell)
    {
        return (cell.Flags & Cell.Flag.IsInteriorCell) == 0;
    }

    public static bool IsPublic(this ICellGetter cell)
    {
        return (cell.Flags & Cell.Flag.PublicArea) != 0;
    }

    /// <summary>
    /// Returns all placed objects in a cell, based on the load order.
    /// All references that are overridden by a higher priority mod are excluded.
    /// </summary>
    /// <param name="cell">Cell to get placed from</param>
    /// <param name="linkCache">Link cache to determine the load order</param>
    /// <param name="includeDeleted">Whether to exclude deleted references</param>
    /// <returns>All placed objects in the cell</returns>
    public static IEnumerable<IPlacedGetter> GetAllPlaced(this ICellGetter cell, ILinkCache linkCache, bool includeDeleted = false)
    {
        var allCells = linkCache.ResolveAll<ICellGetter>(cell.FormKey).ToArray();

        return PlacedObjectsImpl()
            .Where(x => includeDeleted || !x.IsDeleted)
            .DistinctBy(x => x.FormKey);

        IEnumerable<IPlacedGetter> PlacedObjectsImpl()
        {
            foreach (var cellGetter in allCells)
            {
                foreach (var placed in cellGetter.Temporary.Concat(cellGetter.Persistent))
                {
                    yield return placed;
                }
            }
        }
    }
}
