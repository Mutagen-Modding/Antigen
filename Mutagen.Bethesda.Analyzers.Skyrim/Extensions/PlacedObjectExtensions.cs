using System.Diagnostics.CodeAnalysis;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class PlacedObjectExtensions
{
    public static bool LeadsToExterior(this IPlacedObjectGetter placedDoor, ILinkCache linkCache, [MaybeNullWhen(false)] out IPlacedObjectGetter exteriorDoor)
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
}
