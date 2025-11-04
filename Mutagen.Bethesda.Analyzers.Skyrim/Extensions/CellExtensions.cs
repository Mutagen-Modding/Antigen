using Mutagen.Bethesda.Plugins;
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
        var context = linkCache.ResolveSimpleContext(cell);
        return context.Parent?.Record as IWorldspaceGetter;
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
