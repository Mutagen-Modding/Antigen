using System.Diagnostics.CodeAnalysis;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class PlacedObjectExtensions
{
    /// <summary>
    /// Checks if the placed door leads to an exterior cell and returns the exterior door if it does.
    /// </summary>
    /// <param name="placedDoor">Placed door to check.</param>
    /// <param name="linkCache">Link cache to resolve references.</param>
    /// <param name="exteriorDoor">Output parameter that will contain the exterior door if the placed door leads to an exterior cell.</param>
    /// <returns>True if the placed door leads to an exterior cell, false otherwise.</returns>
    public static bool LeadsToExterior(
        this IPlacedObjectGetter placedDoor,
        ILinkCache linkCache,
        [MaybeNullWhen(false)] out IPlacedObjectGetter exteriorDoor)
    {
        exteriorDoor = null;

        // Has a teleport destination
        if (placedDoor.TeleportDestination is null || placedDoor.TeleportDestination.Door.IsNull) return false;
        // Teleport destination is a door
        if (!linkCache.TryResolve<IDoorGetter>(placedDoor.Base.FormKey, out _)) return false;

        if (placedDoor.TeleportDestination.Door.TryResolveSimpleContext(linkCache, out var destinationContext)
            && destinationContext.Parent?.Record is ICellGetter destinationCell)
        {
            exteriorDoor = destinationContext.Record;
            return destinationCell.IsExteriorCell();
        }

        return false;
    }

    public static bool LeadsToExterior(this IPlacedObjectGetter placedDoor, ILinkCache linkCache)
    {
        return LeadsToExterior(placedDoor, linkCache, out _);
    }

    public static bool IsMerchantChest(this IPlacedObjectGetter placedObject, ILinkCache linkCache)
    {
        return linkCache.PriorityOrder.WinningOverrides<IFactionGetter>()
            .Any(faction => faction.Flags.HasFlag(Faction.FactionFlag.Vendor) && faction.MerchantContainer.FormKey == placedObject.FormKey);
    }

    public static bool IsBed(this IPlacedObjectGetter placedObject, ILinkCache linkCache)
    {
        var furnitureFormKey = placedObject.Base.FormKey;

        if (!linkCache.TryResolve<IFurnitureGetter>(furnitureFormKey, out var furniture)) return false;

        if (furniture.Markers is null) return false;
        return furniture.Markers.Any(m =>
            m.EntryPoints != null
            && (m.EntryPoints.Type & Furniture.AnimationType.Lay) != 0);
    }

    public static bool HasLocationRefType(this IPlacedObjectGetter placedObject, FormLink<ILocationReferenceTypeGetter> locRefType)
    {
        return placedObject.LocationRefTypes is not null
               && placedObject.LocationRefTypes.Any(r => r.FormKey == locRefType.FormKey);
    }

    public static IPlacedGetter? GetLinkedReference(this IPlacedObjectGetter placedObject, ILinkCache linkCache, IFormLinkGetter<IKeywordGetter>? keyword = null)
    {
        if (keyword is null)
        {
            foreach (var linkedRef in placedObject.LinkedReferences)
            {
                if (linkedRef.KeywordOrReference.IsNull)
                {
                    var placed = linkedRef.Reference.TryResolve<IPlacedGetter>(linkCache);
                    if (placed is not null)
                    {
                        return placed;
                    }
                }
                else
                {
                    // In case keyword or reference is not null, we check if it's a reference
                    var placed = linkedRef.KeywordOrReference.TryResolve<IPlacedGetter>(linkCache);
                    if (placed is not null)
                    {
                        return placed;
                    }
                }
            }
        }
        else
        {
            foreach (var linkedRef in placedObject.LinkedReferences)
            {
                if (linkedRef.KeywordOrReference.FormKey == keyword.FormKey)
                {
                    return linkedRef.Reference.TryResolve<IPlacedGetter>(linkCache);
                }
            }
        }

        return null;
    }

    public static (IScriptEntryGetter? Script, TProperty? Property) GetScriptPropertyFromSelfOrBase<TProperty>(
        this IPlacedObjectGetter placed,
        ILinkCache linkCache,
        string scriptName,
        string propertyName)
        where TProperty : class, IScriptPropertyGetter
    {
        var script = placed.GetScript(scriptName);
        if (script?.Flags is ScriptEntry.Flag.Removed or ScriptEntry.Flag.InheritedAndRemoved) return (null, null);

        var property = script?.GetProperty<TProperty>(propertyName);
        if (property is not null)
        {
            return (script, property.Flags == ScriptProperty.Flag.Removed ? null : property);
        }

        // Didn't find the property on the placed object directly - check base object
        var baseObject = placed.Base.TryResolve(linkCache);
        if (baseObject is IHaveVirtualMachineAdapterGetter scriptedBaseObject)
        {
            script = scriptedBaseObject.GetScript(scriptName);
            if (script is not null)
            {
                property = script.GetProperty<TProperty>(propertyName);
            }
        }

        return (script, property?.Flags == ScriptProperty.Flag.Removed ? null : property);
    }

    public static ICellGetter? GetCell(this IPlacedGetter placed, ILinkCache linkCache)
    {
        if (!placed.ToLink().TryResolveSimpleContext(linkCache, out var context)) return null;
        if (context.Parent?.Record is not ICellGetter cell) return null;

        if (cell.IsInteriorCell()) return cell;

        if (!context.TryGetParent<IWorldspaceGetter>(out var worldspace)) return null;

        var cellCoordinates = placed.GetCellCoordinates();
        if (cellCoordinates is null) return null;

        return worldspace.GetCell(cellCoordinates.Value);
    }
}
