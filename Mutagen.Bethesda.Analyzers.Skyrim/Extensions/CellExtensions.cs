using Mutagen.Bethesda.Analyzers.Skyrim.Caches;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class CellExtensions
{
    public const int CellLength = 4096;

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
            var worldspace = cell.GetWorldspace(linkCache);
            if (worldspace is not null)
            {
                foreach (var worldLocation in worldspace.GetWorldLocations(linkCache))
                {
                    cellLocations.Add(worldLocation);
                }
            }
        }

        return cellLocations;
    }

    /// <summary>
    /// Estimates if a cell is just a testing cell that can be ignored.
    /// A testing cell is always an interior cell, and has no special setup.
    /// </summary>
    /// <param name="cell">Cell to check</param>
    /// <returns>True if the cell is likely a testing cell</returns>
    public static bool IsTestingCell(this ICellGetter cell)
    {
        if (cell.IsExteriorCell()) return false;
        if (!cell.LockList.IsNull) return false;
        if (!cell.Location.IsNull) return false;
        if (!cell.Owner.IsNull) return false;

        return true;
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

    public static IWorldspaceGetter? GetWorldspace(this ICellGetter cell, ILinkCache linkCache)
    {
        linkCache.TryResolveSimpleContext(cell, out var context);
        if (context == null)
            return null;
        context.TryGetParent<IWorldspaceGetter>(out var world);
        return world;
    }

    public static bool IsInBorderRegion(this ICellGetter cell, ILinkCache linkCache, ILinkUsageCache usageCache)
    {
        if (cell.Regions != null && cell.Regions.Any(r => r.TryResolve(linkCache, out var region) && region.MajorFlags.HasFlag(Region.MajorFlag.BorderRegion)))
            return true;

        // All cells in a worldspace without any border regions are considered part of the playable area
        var world = cell.GetWorldspace(linkCache);
        if (world == null) return true;

        var worldHasBorderRegion = usageCache.GetUsagesOf<IRegionGetter>(world).UsageLinks
            .Select(r => r.Resolve(linkCache))
            .Any(r => r.MajorFlags.HasFlag(Region.MajorFlag.BorderRegion));
        return !worldHasBorderRegion;
    }

    /// <summary>
    /// Get whether the cell is part of a border region or near a cell that is
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="linkCache"></param>
    /// <param name="usageCache"></param>
    /// <param name="exteriorCache"></param>
    /// <param name="maxDistance">Maximum Chebyshev distance (king moves) in cells from this cell</param>
    /// <returns></returns>
    public static bool IsNearBorderRegion(this ICellGetter cell, ILinkCache linkCache, ILinkUsageCache usageCache, IExteriorCellCache exteriorCache, int maxDistance = 2)
    {
        var world = cell.GetWorldspace(linkCache);
        if (world == null || cell.Grid == null)
            return false;

        for (int x = -maxDistance; x <= maxDistance; x++)
        {
            for (int y = -maxDistance; y <= maxDistance; y++)
            {
                var nearby = exteriorCache.GetExterior(world, cell.Grid.Point + new P2Int(x, y));
                if (nearby.TryResolve(linkCache, out var nearbyCell) && nearbyCell.IsInBorderRegion(linkCache, usageCache))
                    return true;
            }
        }
        return false;
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
            .DistinctBy(x => x.FormKey)
            .Where(x => includeDeleted || !x.IsDeleted);

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

    /// <summary>
    /// Returns the winning override of the cell's landscape
    /// </summary>
    /// <param name="cell">Cell to get placed from</param>
    /// <param name="linkCache">Link cache to determine the load order</param>
    /// <returns>Winning landscape override, if it exists</returns>
    public static ILandscapeGetter? GetLandscape(this ICellGetter cell, ILinkCache linkCache)
    {
        var allCells = linkCache.ResolveAll<ICellGetter>(cell.FormKey, ResolveTarget.Winner);

        return allCells.Select(c => c.Landscape).FirstOrDefault(l => l != null);
    }

    /// <summary>
    /// Finds all doors from a given interior cell to the next exterior cell.
    /// Linked interior cells are traversed recursively until an exterior cell is found.
    /// </summary>
    /// <param name="cell">Interior cell to start from</param>
    /// <param name="linkCache">Link cache to resolve cell links</param>
    /// <returns>All doors leading to an exterior cell</returns>
    public static IEnumerable<IPlacedObjectGetter> GetExteriorDoorsGoingIntoInteriorRecursively(this ICellGetter cell, ILinkCache linkCache)
    {
        HashSet<FormKey> visitedCells = [cell.FormKey];
        var queue = new Queue<ICellGetter>();
        queue.Enqueue(cell);

        while (queue.Count > 0)
        {
            var currentCell = queue.Dequeue();

            foreach (var placedObject in currentCell.GetAllPlaced(linkCache).OfType<IPlacedObjectGetter>())
            {
                // Has a teleport destination
                if (placedObject.TeleportDestination is null || placedObject.TeleportDestination.Door.IsNull) continue;

                // Teleport destination is a door
                if (!linkCache.TryResolve<IDoorGetter>(placedObject.Base.FormKey, out _)) continue;

                if (placedObject.TeleportDestination.Door.TryResolveSimpleContext(linkCache, out var destinationDoor)
                    && destinationDoor.Parent?.Record is ICellGetter destinationCell)
                {
                    if (destinationCell.IsInteriorCell())
                    {
                        if (visitedCells.Add(destinationCell.FormKey))
                        {
                            queue.Enqueue(destinationCell);
                        }
                    }
                    else
                    {
                        yield return destinationDoor.Record;
                    }
                }
            }
        }
    }
}
